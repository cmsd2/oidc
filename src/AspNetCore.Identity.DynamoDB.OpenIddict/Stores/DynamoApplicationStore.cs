using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Util;
using AspNetCore.Identity.DynamoDB.OpenIddict.Models;
using OpenIddict.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DynamoDB.OpenIddict
{
    public class DynamoApplicationStore<TApplication, TToken> : IOpenIddictApplicationStore<TApplication>
        where TApplication: DynamoIdentityApplication
        where TToken: DynamoIdentityToken
    {
        private IDynamoDBContext _context;
        private DynamoTokenStore<TToken> _tokenStore;

        public DynamoApplicationStore(IOpenIddictTokenStore<TToken> tokenStore)
        {
            _tokenStore = tokenStore as DynamoTokenStore<TToken>;
        }

        public async Task<TApplication> CreateAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _context.SaveAsync(application, cancellationToken);

            return application;
        }

        public async Task DeleteAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            cancellationToken.ThrowIfCancellationRequested();

            application.Delete();

            await _context.SaveAsync(application);
        }

        public async Task<TApplication> FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var search = _context.FromQueryAsync<TApplication>(new QueryOperationConfig
            {
                IndexName = "ClientId-DeletedOn-index",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "ClientId = :id AND DeletedOn = :deletedOn",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":id", identifier },
                        { ":deletedOn", default(DateTimeOffset).ToString("o") }
                    }
                },
                Limit = 1
            });
            var applications = await search.GetRemainingAsync(cancellationToken);
            return applications?.FirstOrDefault();
        }

        public Task<TApplication> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _context.LoadAsync<TApplication>(identifier, cancellationToken);
        }

        public async Task<TApplication> FindByLogoutRedirectUri(string url, CancellationToken cancellationToken)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var search = _context.FromQueryAsync<TApplication>(new QueryOperationConfig
            {
                IndexName = "LogoutRedirectUri-DeletedOn-index",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "LogoutRedirectUri = :url AND DeletedOn = :deletedOn",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":url", url },
                        { ":deletedOn", default(DateTimeOffset).ToString("o") }
                    }
                },
                Limit = 1
            });
            var applications = await search.GetRemainingAsync(cancellationToken);
            return applications?.FirstOrDefault();
        }

        public async Task<IEnumerable<TApplication>> FindAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var search = _context.FromScanAsync<TApplication>(new ScanOperationConfig
            {
            });

            return await search.GetRemainingAsync(cancellationToken);
        }

        public Task<string> GetClientIdAsync(TApplication application, CancellationToken cancellationToken)
        {
            return Task.FromResult(application.ClientId);
        }

        public Task<string> GetClientTypeAsync(TApplication application, CancellationToken cancellationToken)
        {
            return Task.FromResult(application.Type);
        }

        public Task<string> GetDisplayNameAsync(TApplication application, CancellationToken cancellationToken)
        {
            return Task.FromResult(application.DisplayName);
        }

        public Task<string> GetHashedSecretAsync(TApplication application, CancellationToken cancellationToken)
        {
            return Task.FromResult(application.ClientSecret);
        }

        public Task<string> GetIdAsync(TApplication application, CancellationToken cancellationToken)
        {
            return Task.FromResult(application.Id);
        }

        public Task<string> GetLogoutRedirectUriAsync(TApplication application, CancellationToken cancellationToken)
        {
            return Task.FromResult(application.LogoutRedirectUri);
        }

        public Task<string> GetRedirectUriAsync(TApplication application, CancellationToken cancellationToken)
        {
            return Task.FromResult(application.RedirectUri);
        }

        public async Task<IEnumerable<string>> GetTokensAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var tokens = await _tokenStore.FindByApplicationAsync(application.Id, cancellationToken);

            return tokens.Select(t => t.Id);
        }

        public Task SetClientTypeAsync(TApplication application, string type, CancellationToken cancellationToken)
        {
            application.Type = type;

            return Task.FromResult(0);
        }

        public Task SetHashedSecretAsync(TApplication application, string hash, CancellationToken cancellationToken)
        {
            application.ClientSecret = hash;

            return Task.FromResult(0);
        }

        public async Task UpdateAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _context.SaveAsync(application, cancellationToken);
        }

        public Task EnsureInitializedAsync(IAmazonDynamoDB client, IDynamoDBContext context,
            string applicationTableName = Constants.DefaultApplicationTableName)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;

            if (applicationTableName != Constants.DefaultApplicationTableName)
            {
                AWSConfigsDynamoDB.Context.AddAlias(new TableAlias(applicationTableName, Constants.DefaultTokenTableName));
            }

            return EnsureInitializedImplAsync(client, applicationTableName);
        }

        private async Task EnsureInitializedImplAsync(IAmazonDynamoDB client, string applicationTableName)
        {
            var defaultProvisionThroughput = new ProvisionedThroughput
            {
                ReadCapacityUnits = 5,
                WriteCapacityUnits = 5
            };
            var globalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                new GlobalSecondaryIndex
                {
                    IndexName = "ClientId-DeletedOn-index",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("ClientId", KeyType.HASH),
                        new KeySchemaElement("DeletedOn", KeyType.RANGE)
                    },
                    ProvisionedThroughput = defaultProvisionThroughput,
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL
                    }
                },
                new GlobalSecondaryIndex
                {
                    IndexName = "LogoutRedirectUri-DeletedOn-index",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("LogoutRedirectUri", KeyType.HASH),
                        new KeySchemaElement("DeletedOn", KeyType.RANGE)
                    },
                    ProvisionedThroughput = defaultProvisionThroughput,
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL
                    }
                }
            };

            var tablesResponse = await client.ListTablesAsync();
            if (tablesResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Couldn't get list of tables");
            }
            var tableNames = tablesResponse.TableNames;

            if (!tableNames.Contains(applicationTableName))
            {
                await CreateTableAsync(client, applicationTableName, defaultProvisionThroughput, globalSecondaryIndexes);
                return;
            }

            var response = await client.DescribeTableAsync(new DescribeTableRequest { TableName = applicationTableName });
            var table = response.Table;

            var indexesToAdd =
                globalSecondaryIndexes.Where(
                    g => !table.GlobalSecondaryIndexes.Exists(gd => gd.IndexName.Equals(g.IndexName)));
            var indexUpdates = indexesToAdd.Select(index => new GlobalSecondaryIndexUpdate
            {
                Create = new CreateGlobalSecondaryIndexAction
                {
                    IndexName = index.IndexName,
                    KeySchema = index.KeySchema,
                    ProvisionedThroughput = index.ProvisionedThroughput,
                    Projection = index.Projection
                }
            }).ToList();

            if (indexUpdates.Count > 0)
            {
                await UpdateTableAsync(client, applicationTableName, indexUpdates);
            }
        }

        private async Task CreateTableAsync(IAmazonDynamoDB client, string applicationTableName,
            ProvisionedThroughput provisionedThroughput, List<GlobalSecondaryIndex> globalSecondaryIndexes)
        {
            var response = await client.CreateTableAsync(new CreateTableRequest
            {
                TableName = applicationTableName,
                ProvisionedThroughput = provisionedThroughput,
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "Id",
                        KeyType = KeyType.HASH
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "Id",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "ClientId",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "DeletedOn",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "LogoutRedirectUri",
                        AttributeType = ScalarAttributeType.S
                    }
                },
                GlobalSecondaryIndexes = globalSecondaryIndexes
            });

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Couldn't create table {applicationTableName}");
            }

            await DynamoUtils.WaitForActiveTableAsync(client, applicationTableName);
        }

        private async Task UpdateTableAsync(IAmazonDynamoDB client, string applicationTableName,
            List<GlobalSecondaryIndexUpdate> indexUpdates)
        {
            await client.UpdateTableAsync(new UpdateTableRequest
            {
                TableName = applicationTableName,
                GlobalSecondaryIndexUpdates = indexUpdates
            });

            await DynamoUtils.WaitForActiveTableAsync(client, applicationTableName);
        }
    }
}
