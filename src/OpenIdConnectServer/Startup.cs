using System.IO;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Tokens.Jwt;
using CryptoHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIdConnectServer.Models;
using OpenIdConnectServer.Services;
using OpenIdConnectServer.Extensions;
using PaulMiami.AspNetCore.Authentication.Authenticator;
using PaulMiami.AspNetCore.Identity.Authenticator;
using Directory = OpenIdConnectServer.Services.Directory;
using AspNetCore.Identity.DynamoDB.OpenIddict;
using AspNetCore.Identity.DynamoDB;
using AspNetCore.Identity.DynamoDB.OpenIddict.Stores;
using AspNetCore.Identity.DynamoDB.OpenIddict.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using OpenIddict.Core;
using OpenIddict.DeviceCodeFlow;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace OpenIdConnectServer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<Settings>(Configuration);
            services.Configure<DynamoDbSettings>(Configuration.GetSection("DynamoDB"));
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });
            services.AddSingleton<IConfiguration>(Configuration);

            // Add framework services.
            //            var connection = @"Server=(localdb)\mssqllocaldb;Database=OpenIdConnectServer;Trusted_Connection=True;";
            //            var connection = Configuration.GetConnectionString("DefaultConnection");
            //            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));

            /*services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
                */


            /*services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserManager<OpenLdapUserManager<ApplicationUser>>()
                .AddUserStore<ApplicationUserStore<ApplicationUser,
                    OpenIddictApplication, OpenIddictAuthorization, IdentityRole,
                    OpenIddictToken, ApplicationDbContext, string>>()
                .AddTokenProvider<AuthenticatorTokenProvider<ApplicationUser>>("Totp")
                .AddDefaultTokenProviders();*/
            
            // for ApplicationUserManager
            services.AddSingleton<IPasswordVerifier, DefaultPasswordVerifier>();
            //services.AddSingleton<IUserClaimsPrincipalFactory<ApplicationUser>, UserClaimsPrincipalFactory<ApplicationUser>>();

            services.AddIdentity<ApplicationUser, DynamoIdentityRole>()
                .AddUserManager<ApplicationUserManager<ApplicationUser>>()
                .AddTokenProvider<Services.AuthenticatorTokenProvider<ApplicationUser>>("Authenticator")
                .AddDefaultTokenProviders();

            services.AddDynamoDBIdentity<ApplicationUser, DynamoIdentityRole>()
                .AddUserStore<ApplicationUserStore<ApplicationUser, DynamoIdentityRole>>()
                .AddRoleStore()
                .AddRoleUsersStore();

            services.AddDynamoDBOpenIddictIdentity()
                .AddApplicationStore()
                .AddAuthorizationStore()
                .AddScopeStore()
                .AddDeviceCodeStore()
                .AddTokenStore();

            services.AddSingleton(new DeviceCodeOptions());

            services.AddSingleton<IDeviceCodeStore<DynamoIdentityDeviceCode>,
                DynamoDeviceCodeStore<DynamoIdentityDeviceCode>>();

            services.AddScoped<DeviceCodeManager<DynamoIdentityDeviceCode>>();

            var certPassword = Configuration.GetSection("SigningKey").GetValue<string>("Password", null);
            X509Certificate2 cert = new X509Certificate2(File.ReadAllBytes("cert.pfx"), certPassword);

            services.AddOpenIddict<DynamoIdentityApplication, DynamoIdentityAuthorization, DynamoIdentityScope, DynamoIdentityToken>()
                .AddMvcBinders()
                .AddAuthorizationManager<ApplicationAuthorizationManager<DynamoIdentityAuthorization>>()
                .UseJsonWebTokens()

                // Enable the token endpoint (required to use the password flow).
                .EnableTokenEndpoint("/connect/token")
                .EnableAuthorizationEndpoint("/connect/authorize")
                .EnableLogoutEndpoint("/connect/logout")

                .AllowPasswordFlow()
                .AllowAuthorizationCodeFlow()
                .AllowRefreshTokenFlow()
                .AllowImplicitFlow()
                .AllowClientCredentialsFlow()
                .AllowDeviceCodeFlow()

                // During development, you can disable the HTTPS requirement.
                .DisableHttpsRequirement()

                .UseJsonWebTokens()
                
                .AddSigningCertificate(cert);

            services.AddAuthenticator(c => {
                c.Issuer = "oidc.corp.mendeley.com";
            });

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddScoped<IDirectory, Directory>();
            services.AddScoped<SignInManager<ApplicationUser>, SimpleSignInManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseWhen(httpContext => httpContext.Request.Path.StartsWithSegments("/api"), branch =>
            {
                branch.UseOAuthValidation();

                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
                JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

                branch.UseJwtBearerAuthentication(new JwtBearerOptions
                {
                    Authority = "http://localhost:5000/",
                    Audience = "resource_server", // see also AuthorizationController.CreateTicketAsync and ticket.SetResources
                    RequireHttpsMetadata = false,
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = OpenIdConnectConstants.Claims.Subject,
                        RoleClaimType = OpenIdConnectConstants.Claims.Role
                    }
                });
            });

            app.UseWhen(httpContext => !httpContext.Request.Path.StartsWithSegments("/api"), branch =>
            {
                // Insert a new cookies middleware in the pipeline to store the user
                // identity after he has been redirected from the identity provider.
                branch.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AutomaticAuthenticate = true,
                    AutomaticChallenge = true,
                    LoginPath = new PathString("/Account/Login")
                });

                branch.UseIdentity();
            });

            app.UseOpenIddict();

            var options = app.ApplicationServices.GetService<IOptions<DynamoDbSettings>>();
            var client = env.IsDevelopment()
                ? new AmazonDynamoDBClient(new AmazonDynamoDBConfig
                {
                    ServiceURL = options.Value.ServiceUrl
                })
                : new AmazonDynamoDBClient();
            var context = new DynamoDBContext(client);

            var userStore = app.ApplicationServices
                .GetService<IUserStore<ApplicationUser>>()
                as DynamoUserStore<ApplicationUser, DynamoIdentityRole>;
            var roleStore = app.ApplicationServices
                .GetService<IRoleStore<DynamoIdentityRole>>()
                as DynamoRoleStore<DynamoIdentityRole>;
            var roleUsersStore = app.ApplicationServices
                .GetService<DynamoRoleUsersStore<DynamoIdentityRole, ApplicationUser>>();
            var applicationsStore = app.ApplicationServices
                .GetService<IOpenIddictApplicationStore<DynamoIdentityApplication>>()
                as DynamoApplicationStore<DynamoIdentityApplication, DynamoIdentityToken>;
            var authorizationStore = app.ApplicationServices
                .GetService<IOpenIddictAuthorizationStore<DynamoIdentityAuthorization>>()
                as DynamoAuthorizationStore<DynamoIdentityAuthorization>;
            var scopeStore = app.ApplicationServices
                .GetService<IOpenIddictScopeStore<DynamoIdentityScope>>()
                as DynamoScopeStore<DynamoIdentityScope>;
            var deviceCodeStore = app.ApplicationServices
                .GetService<IDeviceCodeStore<DynamoIdentityDeviceCode>>()
                as DynamoDeviceCodeStore<DynamoIdentityDeviceCode>;
            var tokenStore = app.ApplicationServices
                .GetService<IOpenIddictTokenStore<DynamoIdentityToken>>()
                as DynamoTokenStore<DynamoIdentityToken>;
                

            userStore.EnsureInitializedAsync(client, context, options.Value.UsersTableName).Wait();
            roleStore.EnsureInitializedAsync(client, context, options.Value.RolesTableName).Wait();
            roleUsersStore.EnsureInitializedAsync(client, context, options.Value.RoleUsersTableName).Wait();
            applicationsStore.EnsureInitializedAsync(client, context, options.Value.ApplicationsTableName).Wait();
            authorizationStore.EnsureInitializedAsync(client, context, options.Value.AuthorizationsTableName).Wait();
            scopeStore.EnsureInitializedAsync(client, context, options.Value.ScopesTableName).Wait();
            deviceCodeStore.EnsureInitializedAsync(client, context, options.Value.DeviceCodesTableName).Wait();
            tokenStore.EnsureInitializedAsync(client, context, options.Value.TokensTableName).Wait();

            CreateClientAsync(applicationsStore, "YOUR_CLIENT_APP_ID", "YOUR_CLIENT_APP_SECRET", "http://localhost:5001").Wait();
            CreateClientAsync(applicationsStore, "console", "388D45FA-B36B-4988-BA59-B187D329C207", "http://localhost:5001").Wait();
            CreateClientAsync(applicationsStore, "mvc", "901564A5-E7FE-42CB-B10D-61EF6A8F36", "http://localhost:53507").Wait();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseForwardedHeaders();

            app.UseMvcWithDefaultRoute();
        }

        private async Task CreateClientAsync(
            DynamoApplicationStore<DynamoIdentityApplication,DynamoIdentityToken> applicationsStore, 
            string clientId, string clientSecret, string url)
        {
            var clientApp = applicationsStore.FindByClientIdAsync(clientId, default(CancellationToken)).Result;

            if (clientApp == null)
            {
                clientApp = new DynamoIdentityApplication
                {
                    ClientId = clientId,
                    DisplayName = "My client application",
                    RedirectUri = url + "/signin-oidc",
                    LogoutRedirectUri = url + "/signout-callback-oidc",
                    ClientSecret = Crypto.HashPassword(clientSecret),
                    Type = clientSecret == null 
                        ? OpenIddictConstants.ClientTypes.Public 
                        : OpenIddictConstants.ClientTypes.Confidential
                };

                await applicationsStore.CreateAsync(clientApp, default(CancellationToken));
            }
        }
    }
}
