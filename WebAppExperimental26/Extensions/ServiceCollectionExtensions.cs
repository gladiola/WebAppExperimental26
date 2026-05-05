using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using WebAppExperimental26.Models.Settings;
using WebAppExperimental26.Services;
using WebAppExperimental26.AzureKeyVaultOperations;
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
            }
            else
            {
                services.AddDistributedMemoryCache();
                services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                });

                logger.LogInformation("Session configured with 30-minute timeout");
            }

            return services;
        }

        /// <summary>
        /// Configure localization for en-US, de-DE, es-ES, and fr-FR
        /// </summary>
        public static IServiceCollection AddLocalizationConfiguration(this IServiceCollection services, ILogger logger, bool enabled = true)
        {
            if (!enabled)
            {
                logger.LogWarning("Localization is DISABLED");
            }
            else
            {
                services.AddLocalization(options => options.ResourcesPath = "Resources");

                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("de-DE"),
                    new CultureInfo("es-ES"),
                    new CultureInfo("fr-FR"),
                };

                services.Configure<RequestLocalizationOptions>(options =>
                {
                    options.DefaultRequestCulture = new RequestCulture("en-US");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                });

                logger.LogInformation("Localization configured for en-US, de-DE, es-ES, fr-FR");
            }

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
            }
            else
            {
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
            }

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
            })
            .AddViewLocalization()
            .AddDataAnnotationsLocalization();

            if (enableAuthorization)
            {
                razorPages.AddMicrosoftIdentityUI();
                logger.LogInformation("Razor Pages configured WITH authorization and view localization");
            }
            else
            {
                logger.LogInformation("Razor Pages configured WITHOUT authorization, with view localization");
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
            }
            else
            {
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

                    // Get mTLS settings if available
                    var mtlsSettings = configuration.GetSection("MtlsSettings").Get<MtlsSettings>();
                    var enableMtls = mtlsSettings?.RequireClientCertificate ?? false;

                    // Configure Kestrel
                    builder.WebHost.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ConfigureHttpsDefaults(httpsOptions =>
                        {
                            if (!environment.IsDevelopment())
                            {
                                httpsOptions.ServerCertificate = serverCert;
                                
                                // Configure client certificate mode for mTLS
                                if (enableMtls)
                                {
                                    httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                                    logger.LogInformation("mTLS enabled - Client certificates REQUIRED");
                                }
                                else
                                {
                                    httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                                    logger.LogInformation("mTLS disabled - Client certificates optional");
                                }
                            }
                            else
                            {
                                // In development, make client certificates optional
                                httpsOptions.ServerCertificate = serverCert;
                                httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                                logger.LogInformation("Development mode - Client certificates optional");
                            }
                        });
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Key Vault configuration failed");
                    throw;
                }
            }

            return services;
        }

        /// <summary>
        /// Configure mutual TLS (mTLS) client certificate authentication
        /// </summary>
        public static IServiceCollection AddMtlsAuthentication(
            this IServiceCollection services,
            IConfiguration configuration,
            ILogger logger,
            bool enabled = true)
        {
            if (!enabled)
            {
                logger.LogWarning("mTLS Client Certificate Authentication is DISABLED");
            }
            else
            {
                var mtlsSettings = configuration.GetSection("MtlsSettings").Get<MtlsSettings>()
                    ?? throw new InvalidOperationException("MtlsSettings not found");

                logger.LogInformation("Configuring mTLS certificate authentication");

                // Evaluate and log the configuration eagerly so that tests and operators can
                // see what is configured at startup time (not deferred inside AddCertificate).
                if (mtlsSettings.AllowSelfSignedCertificates && mtlsSettings.AllowCertificateChains)
                {
                    logger.LogWarning("mTLS: Allowing both chained AND self-signed certificates");
                }
                else if (mtlsSettings.AllowSelfSignedCertificates)
                {
                    logger.LogWarning("mTLS: Allowing self-signed certificates only");
                }
                else
                {
                    logger.LogInformation("mTLS: Allowing chained certificates only (recommended)");
                }

                logger.LogInformation("mTLS: Certificate revocation check = {RevocationCheck}",
                    mtlsSettings.CheckCertificateRevocation ? "ENABLED" : "DISABLED");

                services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                    .AddCertificate(options =>
                    {
                        // Configure certificate types
                        if (mtlsSettings.AllowSelfSignedCertificates && mtlsSettings.AllowCertificateChains)
                        {
                            options.AllowedCertificateTypes = CertificateTypes.All;
                        }
                        else if (mtlsSettings.AllowSelfSignedCertificates)
                        {
                            options.AllowedCertificateTypes = CertificateTypes.SelfSigned;
                        }
                        else
                        {
                            options.AllowedCertificateTypes = CertificateTypes.Chained;
                        }

                        // Configure revocation mode
                        options.RevocationMode = mtlsSettings.CheckCertificateRevocation 
                            ? X509RevocationMode.Online 
                            : X509RevocationMode.NoCheck;

                        // Configure authentication events
                        options.Events = new CertificateAuthenticationEvents
                        {
                            OnAuthenticationFailed = context =>
                            {
                                logger.LogError("mTLS Authentication FAILED: {Error}", context.Exception?.Message);
                                return Task.CompletedTask;
                            },
                            OnCertificateValidated = context =>
                            {
                                logger.LogInformation("mTLS Authentication SUCCEEDED for certificate: {Subject}", 
                                    context.ClientCertificate.Subject);
                                
                                if (mtlsSettings.ValidateClientCertificateIssuer)
                                {
                                    var issuer = context.ClientCertificate.Issuer;
                                    if (!mtlsSettings.IsIssuerAllowed(issuer))
                                    {
                                        logger.LogError(
                                            "mTLS: Certificate issuer '{Issuer}' is not in the allowed list. Allowed: [{Allowed}]",
                                            issuer,
                                            string.Join(", ", mtlsSettings.AllowedIssuers ?? new List<string>()));
                                        context.Fail("Certificate issuer not trusted");
                                    }
                                }

                                return Task.CompletedTask;
                            }
                        };
                    });

                logger.LogInformation("mTLS certificate authentication configured successfully");
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
            }
            else
            {
                var nonceSettings = configuration.GetSection("NonceEncryption").Get<NonceEncryptionSettings>()
                    ?? throw new InvalidOperationException("NonceEncryptionSettings not found");

                services.AddSingleton<INonceEncryptionSettingsService>(new NonceEncryptionSettingsService(nonceSettings));
                services.AddSingleton<INonceRefresherService, NonceRefresherService>();
                services.AddSingleton<INonceCatalogService, NonceCatalogService>();

                // CSP Builder
                services.Configure<CSPScriptHashSettings>(configuration.GetSection("CSPScriptHashes"));
                services.AddSingleton<ContentSecurityPolicyBuilder>();

                logger.LogInformation("Nonce and CSP services configured");
            }

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
            }
            else
            {
                var blobSettings = configuration.GetSection("BlobSettings").Get<BlobSettings>()
                    ?? throw new InvalidOperationException("BlobSettings not found");

                if (string.IsNullOrEmpty(blobSettings.BlobConnectionString))
                {
                    throw new InvalidOperationException("BlobConnectionString is null or empty");
                }

                logger.LogInformation("BlobSettings configured with MaxAttachments: {Max}", blobSettings.MaxAttachments);

                services.AddSingleton(blobSettings);
                services.AddScoped<IBlobSettingsService, BlobSettingsService>();
            }

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
            }
            else
            {
                try
                {
                    var cosmosSettings = configuration.GetSection("CosmosDb").Get<CosmosDbSettings>()
                        ?? throw new InvalidOperationException("CosmosDbSettings not found");

                    logger.LogInformation("Cosmos connection string configured: {Present}",
                        !string.IsNullOrEmpty(cosmosSettings.CosmosConnectionString));

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

                    logger.LogInformation("Cosmos DB services configured");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Cosmos DB configuration failed");
                    throw;
                }
            }

            return services;
        }
    }
}