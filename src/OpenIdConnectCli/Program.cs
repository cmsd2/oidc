using System;
using AspNetCore.Identity.DynamoDB;
using AspNetCore.Identity.DynamoDB.OpenIddict;
using AspNetCore.Identity.DynamoDB.OpenIddict.Models;
using System.Threading.Tasks;
using System.Threading;
using OpenIddict.Core;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon;
using Newtonsoft.Json;

namespace OpenIdConnectCli
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             * This cli is only for bootstrapping the DB with
             * an admin user.
             * For all other cases, use the admin features in the
             * auth service.
             * 
             * # use cases:
             * 2. Grant role to user
             * 3. Remove role from user
             * 4. Create admin role
             */

            var loggerFactory = new LoggerFactory();
            loggerFactory
                .AddConsole()
                .AddDebug();

            var keyNormalizer = new UpperInvariantLookupNormalizer();

            var roleUsersStore = new DynamoRoleUsersStore<DynamoIdentityRole, DynamoIdentityUser>();
            var userStore = new DynamoUserStore<DynamoIdentityUser, DynamoIdentityRole>(roleUsersStore);
            var roleStore = new DynamoRoleStore<DynamoIdentityRole>();
            

            var app = new CommandLineApplication();
            app.HelpOption("-? | -h | --help");

            var dbUrl = app.Option("-d|--dynamodb", "DynamoDB endpoint", CommandOptionType.SingleValue);
            var region = app.Option("-R|--region", "AWS Region", CommandOptionType.SingleValue);
            var tableNamePrefix = app.Option("-p|--prefix", "Table Name Prefix", CommandOptionType.SingleValue);

            app.Command("role", roleCommand =>
            {
                roleCommand.HelpOption("-? | -h | --help");

                var roleName = roleCommand.Option("-r|--role <role>", "role name", CommandOptionType.SingleValue);

                roleCommand.Command("add", addRole =>
                {
                    addRole.HelpOption("-? | -h | --help");

                    addRole.OnExecute(() =>
                    {
                        Program program = new Program(
                            loggerFactory,
                            userStore,
                            roleStore,
                            roleUsersStore,
                            keyNormalizer,
                            dbUrl.Value(),
                            region.Value(),
                            tableNamePrefix.Value());

                        program.CreateRole(roleName.Value()).Wait();
                        return 0;
                    });
                });

                roleCommand.Command("remove", removeRole =>
                {
                    removeRole.HelpOption("-? | -h | --help");

                    removeRole.OnExecute(() =>
                    {
                        Program program = new Program(
                            loggerFactory,
                            userStore,
                            roleStore,
                            roleUsersStore,
                            keyNormalizer,
                            dbUrl.Value(),
                            region.Value(),
                            tableNamePrefix.Value());

                        program.RemoveRole(roleName.Value()).Wait();
                        return 0;
                    });
                });
            });

            app.Command("claim", claimCommand =>
            {
                claimCommand.HelpOption("-? | -h | --help");

                var roleName = claimCommand.Option("-r|--role <role>", "role name", CommandOptionType.SingleValue);
                var claimType = claimCommand.Option("-t|--claim-type <type>", "claim type", CommandOptionType.SingleValue);
                var claimValue = claimCommand.Option("-v|--claim-value <value>", "claim value", CommandOptionType.SingleValue);

                claimCommand.Command("get", getClaims =>
                {
                    getClaims.HelpOption("-? | -h | --help");

                    getClaims.OnExecute(() =>
                    {
                        Program program = new Program(
                            loggerFactory,
                            userStore,
                            roleStore,
                            roleUsersStore,
                            keyNormalizer,
                            dbUrl.Value(),
                            region.Value(),
                            tableNamePrefix.Value());
                        
                        var claims = program.GetClaims(roleName.Value()).Result;

                        Console.WriteLine(JsonConvert.SerializeObject(claims, Formatting.Indented));

                        return 0;
                    });
                });

                claimCommand.Command("add", addClaim => 
                {    
                    addClaim.HelpOption("-? | -h | --help");

                    addClaim.OnExecute(() =>
                    {
                        Program program = new Program(
                            loggerFactory,
                            userStore,
                            roleStore,
                            roleUsersStore,
                            keyNormalizer,
                            dbUrl.Value(),
                            region.Value(),
                            tableNamePrefix.Value());

                        var claim = new Claim(claimType.Value(), claimValue.Value());
                        program.AddClaim(roleName.Value(), claim).Wait();
                        return 0;
                    });
                });

                claimCommand.Command("remove", removeClaim => 
                {
                    removeClaim.HelpOption("-? | -h | --help");

                    removeClaim.OnExecute(() =>
                    {
                        Program program = new Program(
                            loggerFactory,
                            userStore,
                            roleStore,
                            roleUsersStore,
                            keyNormalizer,
                            dbUrl.Value(),
                            region.Value(),
                            tableNamePrefix.Value());

                        var claim = new Claim(claimType.Value(), claimValue.Value());
                        program.RemoveClaim(roleName.Value(), claim).Wait();
                        return 0;
                    });
                });
            });

            app.Execute(args);
        }

        private ILogger<Program> logger;
        private DynamoUserStore<DynamoIdentityUser, DynamoIdentityRole> userStore;
        private DynamoRoleStore<DynamoIdentityRole> roleStore;
        private DynamoRoleUsersStore<DynamoIdentityRole, DynamoIdentityUser> roleUsersStore;
        private ILookupNormalizer keyNormalizer;
        private string roleUsersTableName = "roleUsers";
        private string usersTableName = "users";
        private string rolesTableName = "roles";

        public Program(ILoggerFactory loggerFactory,
            DynamoUserStore<DynamoIdentityUser, DynamoIdentityRole> userStore,
            DynamoRoleStore<DynamoIdentityRole> roleStore,
            DynamoRoleUsersStore<DynamoIdentityRole, DynamoIdentityUser> roleUsersStore,
            ILookupNormalizer keyNormalizer,
            string dbUrl,
            string regionName,
            string tableNamePrefix)
        {
            this.logger = loggerFactory.CreateLogger<Program>();
            this.userStore = userStore;
            this.roleStore = roleStore;
            this.roleUsersStore = roleUsersStore;
            this.keyNormalizer = keyNormalizer;

            var dbConfig = new AmazonDynamoDBConfig
            {
                ServiceURL = dbUrl
            };
            if (regionName != null)
            {
                var region = RegionEndpoint.GetBySystemName(regionName);
                dbConfig.RegionEndpoint = region;
            }

            var client = new AmazonDynamoDBClient(dbConfig);
            
            var contextConfig = new DynamoDBContextConfig
            {
                TableNamePrefix = tableNamePrefix
            };

            var context = new DynamoDBContext(client, contextConfig);

            var prefix = tableNamePrefix ?? "";

            roleUsersTableName = $"{prefix}roleUsers";
            usersTableName = $"{prefix}users";
            rolesTableName = $"{prefix}roles";

            var tables = client.ListTablesAsync().Result;

            if (!tables.TableNames.Contains(usersTableName))
            {
                throw new Exception($"can't find table {usersTableName}");
            }

            if (!tables.TableNames.Contains(rolesTableName))
            {
                throw new Exception($"can't find table {rolesTableName}");
            }

            if (!tables.TableNames.Contains(roleUsersTableName))
            {
                throw new Exception($"can't find table {roleUsersTableName}");
            }

            roleUsersStore.EnsureInitializedAsync(client, context, roleUsersTableName).Wait();
            userStore.EnsureInitializedAsync(client, context, usersTableName).Wait();
            roleStore.EnsureInitializedAsync(client, context, rolesTableName).Wait();
        }

        public async Task AddUserToRole(string id, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var normalizedRoleName = keyNormalizer.Normalize(roleName);

            var user = await userStore.FindByIdAsync(id, cancellationToken);

            if (user == null)
            {
                throw new Exception($"user {id} not found");
            }

            await userStore.AddToRoleAsync(user, normalizedRoleName, cancellationToken);
        }

        public async Task RemoveUserFromRole(string id, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var normalizedRoleName = keyNormalizer.Normalize(roleName);

            var user = await userStore.FindByIdAsync(id, cancellationToken);

            if (user == null)
            {
                throw new Exception($"user {id} not found");
            }

            await userStore.RemoveFromRoleAsync(user, normalizedRoleName, cancellationToken);
        }

        public async Task<DynamoIdentityRole> CreateRole(string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var role = new DynamoIdentityRole(roleName);

            await roleStore.CreateAsync(role, cancellationToken);

            return role;
        }

        public async Task RemoveRole(string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var role = await roleStore.FindByNameAsync(keyNormalizer.Normalize(roleName), cancellationToken);

            if (role == null)
            {
                throw new Exception($"role named {roleName} not found");
            }

            await roleStore.DeleteAsync(role, cancellationToken);
        }

        public async Task AddClaim(string roleName, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            var role = await roleStore.FindByNameAsync(keyNormalizer.Normalize(roleName), cancellationToken);

            if (role == null)
            {
                throw new Exception($"role named {roleName} not found");
            }

            await roleStore.AddClaimAsync(role, claim, cancellationToken);

            await roleStore.UpdateAsync(role, cancellationToken);
        }

        public async Task RemoveClaim(string roleName, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            var role = await roleStore.FindByNameAsync(keyNormalizer.Normalize(roleName), cancellationToken);

            if (role == null)
            {
                throw new Exception($"role named {roleName} not found");
            }

            await roleStore.RemoveClaimAsync(role, claim, cancellationToken);

            await roleStore.UpdateAsync(role, cancellationToken);
        }

        public async Task<IList<Claim>> GetClaims(string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var role = await roleStore.FindByNameAsync(keyNormalizer.Normalize(roleName), cancellationToken);

            if (role == null)
            {
                throw new Exception($"role named {roleName} not found");
            }

            return await roleStore.GetClaimsAsync(role, cancellationToken);
        }
    }
}