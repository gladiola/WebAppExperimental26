using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Localization;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using WebAppExperimental26.Models.Settings;
using WebAppExperimental26.Services;
using WebAppExperimental26.AzureKeyVaultOperations;
using WebAppExperimental26.Models.Storage;
using WebAppExperimental26.Interfaces.Main_Objects;

namespace WebAppExperimental26.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure feature flags from appsettings
        /// </summary>
        public static IServiceCollection AddFeatureFlags(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FeatureFlags>(configuration.GetSection("FeatureFlags"));
            return services;
        }

        /// <summary>
        /// Add session and distributed cache
        /// </summary>
        public static IServiceCollection AddSessionConfiguration(this IServiceCollection services, ILogger logger, bool enabled = true)
        {
            if (!enabled)
            {
                logger.LogWarning("Session is DISABLED");
                return services;
            }

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            logger.LogInformation("Session configured with 30-minute timeout");
            return services;
        }

        /// <summary>
        /// Configure localization to en-US only
        /// </summary>
        public static IServiceCollection AddLocalizationConfiguration(this IServiceCollection services, ILogger logger, bool enabled = true)
        {
            if (!enabled)
            {
                logger.LogWarning("Localization is DISABLED");
                return services;
            }

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCulture = new[] { new CultureInfo("en-US") };
                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = supportedCulture;
                options.SupportedUICultures = supportedCulture;
            });

            logger.LogInformation("Localization configured for en-US only");
            return services;
        }

        /// <summary>
        /// Configure Azure AD authentication and authorization
        /// </summary>
        public static IServiceCollection AddAzureAdAuthentication(
            this IServiceCollection services,
            IConfiguration configuration,
            ILogger logger,
            bool enabled = true)
        {
            if (!enabled)
            {
                logger.LogWarning("Azure AD Authentication is DISABLED");
                return services;
            }

            var adSettings = configuration.GetSection("AzureAD").Get<AzureADSettings>()
                ?? throw new InvalidOperationException("AzureADSettings not found");

            logger.LogInformation("Configuring Azure AD with ClientId: {ClientId}", adSettings.ClientId);

            services.AddSingleton<IAzureADSettingsService>(new AzureADSettingsService(adSettings));

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(configuration.GetSection("AzureAd"));

            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    logger.LogError("Redirect to login for path: {Path}", context.Request.Path);
                    return Task.CompletedTask;
                };
            });

            services.AddAuthorization();

            return services;
        }

        /// <summary>
        /// Configure Razor Pages with authorization policies
        /// </summary>
        public static IServiceCollection AddRazorPagesConfiguration(
            this IServiceCollection services,
            ILogger logger,
            bool enableAuthorization = true)
        {
            var razorPages = services.AddRazorPages(options =>
            {
                if (enableAuthorization)
                {
                    options.Conventions.AuthorizeFolder("/Experimental");

                }

                options.Conventions.AllowAnonymousToPage("/Privacy");
                options.Conventions.AllowAnonymousToPage("/Error");
                options.Conventions.AllowAnonymousToPage("/About");
            });

            if (enableAuthorization)
            {
                razorPages.AddMicrosoftIdentityUI();
                logger.LogInformation("Razor Pages configured WITH authorization");
            }
            else
            {
                logger.LogInformation("Razor Pages configured WITHOUT authorization");
            }

            return services;
        }

        /// <summary>
        /// Configure Azure Key Vault operations
        /// </summary>
        public static async Task<IServiceCollection> AddKeyVaultServicesAsync(
            this IServiceCollection services,
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            IWebHostEnvironment environment,
            WebApplicationBuilder builder,
            bool enabled = true)
        {
            if (!enabled)
            {
                loggerFactory.CreateLogger("KeyVault").LogWarning("Key Vault integration is DISABLED");
                return services;
            }

            var logger = loggerFactory.CreateLogger("KeyVault");

            try
            {
                var kvSettings = configuration.GetSection("AzureKeyVault").Get<KeyVaultSettings>()
                    ?? throw new InvalidOperationException("KeyVaultSettings not found");

                var adSettings = configuration.GetSection("AzureAD").Get<AzureADSettings>()
                    ?? throw new InvalidOperationException("AzureADSettings not found for Key Vault");

                services.AddSingleton<IKeyVaultSettingsService>(new KeyVaultSettingsService(kvSettings));
                services.AddSingleton<IAzureKeyVaultCertificateOperations, AzureKeyVaultCertificateOperations>();
                services.AddSingleton<IAzureKeyVaultOperationsService, AzureKeyVaultOperationsService>();

                // Retrieve certificate
                var akvo = new AzureKeyVaultCertificateOperations(loggerFactory.CreateLogger<AzureKeyVaultCertificateOperations>());
                
                var clientSecret = adSettings.ClientCredentials.FirstOrDefault(cc => cc.SourceType == "ClientSecret")?.ClientSecret ?? string.Empty;
                
                var serverCert = await akvo.GetCertificateFromKeyVault(
                    adSettings.TenantId,
                    adSettings.ClientId,
                    kvSettings.KeyVaultURL,
                    kvSettings.KeyVaultSecret,
                    kvSettings.KeyVaultPassName);

                if (serverCert == null)
                {
                    throw new InvalidOperationException("Server certificate not retrieved from Key Vault");
                }

                logger.LogInformation("Server certificate retrieved from Key Vault");

                // Configure Kestrel
                builder.WebHost.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
                    {
                        if (!environment.IsDevelopment())
                        {
                            httpsOptions.ServerCertificate = serverCert;
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Key Vault configuration failed");
                throw;
            }

            return services;
        }

        /// <summary>
        /// Configure nonce-based CSP services
        /// </summary>
        public static IServiceCollection AddNonceServices(
            this IServiceCollection services,
            IConfiguration configuration,
            ILogger logger,
            bool enabled = true)
        {
            if (!enabled)
            {
                logger.LogWarning("Nonce Services are DISABLED");
                return services;
            }

            var nonceSettings = configuration.GetSection("NonceEncryption").Get<NonceEncryptionSettings>()
                ?? throw new InvalidOperationException("NonceEncryptionSettings not found");

            services.AddSingleton<INonceEncryptionSettingsService>(new NonceEncryptionSettingsService(nonceSettings));
            services.AddSingleton<INonceRefresherService, NonceRefresherService>();
            services.AddSingleton<INonceCatalogService, NonceCatalogService>();

            // CSP Builder
            services.Configure<CSPScriptHashSettings>(configuration.GetSection("CSPScriptHashes"));
            services.AddSingleton<ContentSecurityPolicyBuilder>();

            logger.LogInformation("Nonce and CSP services configured");
            return services;
        }

        /// <summary>
        /// Configure Azure Blob Storage
        /// </summary>
        public static IServiceCollection AddBlobStorageServices(
            this IServiceCollection services,
            IConfiguration configuration,
            ILogger logger,
            bool enabled = true)
        {
            if (!enabled)
            {
                logger.LogWarning("Blob Storage is DISABLED");
                return services;
            }

            var blobSettings = configuration.GetSection("BlobSettings").Get<BlobSettings>()
                ?? throw new InvalidOperationException("BlobSettings not found");

            if (string.IsNullOrEmpty(blobSettings.BlobConnectionString))
            {
                throw new InvalidOperationException("BlobConnectionString is null or empty");
            }

            logger.LogInformation("BlobSettings configured with MaxAttachments: {Max}", blobSettings.MaxAttachments);

            services.AddSingleton(blobSettings);
            services.AddScoped<IBlobSettingsService, BlobSettingsService>();

            return services;
        }

        /// <summary>
        /// Configure Cosmos DB
        /// </summary>
        public static async Task<IServiceCollection> AddCosmosDbServicesAsync(
            this IServiceCollection services,
            IConfiguration configuration,
            ILogger logger,
            bool enabled = true)
        {
            if (!enabled)
            {
                logger.LogWarning("Cosmos DB is DISABLED");
                return services;
            }

            try
            {
                var cosmosSettings = configuration.GetSection("CosmosDb").Get<CosmosDbSettings>()
                    ?? throw new InvalidOperationException("CosmosDbSettings not found");

                logger.LogInformation("Cosmos connection string sample (last 5): {Sample}",
                    cosmosSettings.CosmosConnectionString[^5..]);

                // Verify connection
                var client = new CosmosClient(cosmosSettings.CosmosConnectionString);
                var database = client.GetDatabase(cosmosSettings.DatabaseName);
                await database.ReadAsync();

                logger.LogInformation("Cosmos DB connection verified: {Endpoint}", client.Endpoint);

                services.AddSingleton<ICosmosDbSettingsService>(new CosmosDbSettingsService(cosmosSettings));
                services.AddSingleton(client);
                services.AddSingleton<CosmosDbService>(provider =>
                {
                    var dbClient = provider.GetRequiredService<CosmosClient>();
                    return new CosmosDbService(dbClient, cosmosSettings.DatabaseName);
                });

                services.AddDbContext<RedCosmosDBContext>((sp, options) => options.UseCosmos(
                    cosmosSettings.AccountEndpoint,
                    cosmosSettings.AccountKey,
                    cosmosSettings.DatabaseName));

                logger.LogInformation("Cosmos DB services configured");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Cosmos DB configuration failed");
                throw;
            }

            return services;
        }
    }
}