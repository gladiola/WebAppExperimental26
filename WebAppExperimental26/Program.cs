

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;
using NuGet.Configuration;
using REDRFID.AzureKeyVaultOperations;
using REDRFID.Data;
using REDRFID.Interfaces.Main_Objects;
using REDRFID.Models.Main_Objects;
using REDRFID.Models.Settings;
using REDRFID.Models.Storage;
using REDRFID.Services;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using WebAppExperimental26.Models.Main_Objects;
using WebAppExperimental26.Models.Settings;
using WebAppExperimental26.Models.Storage;

namespace REDRFID
{
    public class Program


    {
        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            var environment = builder.Environment;

            #region Begin Logging

            // Replace the loggerFactory creation block with the following:
            var loggerFactory = LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddAzureWebAppDiagnostics(); // Ensure the required NuGet package is installed
            });

            var logger = loggerFactory.CreateLogger("Program");
            logger.LogInformation("Program.Main logging activated.");

            string caller = "Program.Main()";
            LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, DataProcessingStatus.Info, $"beginning use of LoggingHelper methods");

            #endregion

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                //options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            #region Limit Internationalization to English
            // We don't speak other languages well enough to support them.  
            // Turn off elements we don't understand.

            builder.Services.Configure<RequestLocalizationOptions>(options => {

                var supportedCulture = new[] { new CultureInfo("en-US") };
                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = supportedCulture;
                options.SupportedUICultures = supportedCulture;

            });

            #endregion



            #region builder Authentication and Authorization

            // The AzureAD values from the service, above,
            // are not getting stitched in properly here.
            // Adjust. 

            /*

            // Configure Microsoft Identity with settings from the service
            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    options.Instance = adss.Authority;
                    options.Domain = adss.Domain;
                    options.TenantId = adss.TenantId;
                    options.ClientId = adss.ClientId;
                    options.ClientSecret = adss.ClientCredentials.FirstOrDefault(c => !string.IsNullOrEmpty(c.ClientSecret))?.ClientSecret;
                    options.CallbackPath = "/signin-oidc"; // Adjust as necessary
                })
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();

            */

            AzureADSettings adss = builder.Configuration.GetSection("AzureAD").Get<AzureADSettings>()! ?? throw new InvalidOperationException("Values for AzureADSettings not found.")!;

            LoggingHelper.LogDataProcessingStatusServiceWork(logger, "Program.Main", string.Empty, DataProcessingStatus.Info, $"AzureADSettings ClientID check: {adss.ClientId}");


            builder.Services.AddSingleton<IAzureADSettingsService>(new AzureADSettingsService(adss));

            builder.Services.ConfigureApplicationCookie(options =>
            {
                /*
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(logger, "OnRedirectToAccessDenied", string.Empty, DataProcessingStatus.Error, $"AzureADSettings ClientID check: {adss.ClientId} Context Request Path:  {context.Request.Path.ToString()}");

                    //context.Response.StatusCode = StatusCodes.Status403Forbidden;

                    

                    return Task.CompletedTask;
                };

                */

                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    LoggingHelper.LogDataProcessingStatusServiceWork(logger, "OnRedirectToLogin ", string.Empty, DataProcessingStatus.Error, $"AzureADSettings ClientID check: {adss.ClientId} Context Request Path:  {context.Request.Path.ToString()}");

                    return Task.CompletedTask;
                };
            });

            /*
            System.InvalidOperationException: 'No authentication handler is registered for the scheme 'OpenIdConnect'. The registered schemes are: Bearer, Certificate. Did you forget to call AddAuthentication().Add[SomeAuthHandler]("OpenIdConnect",...)?'
            */

            /*
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)  // Set JWT as the default authentication scheme
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = adss.Authority,  // Set this to your actual issuer
                        ValidateAudience = true,
                        ValidAudience = adss.ClientId, // Set this to your actual audience (Client ID)
                        ValidateLifetime = true,
                        //ValidateIssuerSigningKey = true,
                       // IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // Your signing key
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            // Handle authentication failures
                            LoggingHelper.LogDataProcessingStatusServiceWork(logger, "OnAuthenticationFailed", DataProcessingStatus.Failure, "Authentication failed.");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            // Custom logic on successful token validation
                            LoggingHelper.LogDataProcessingStatusServiceWork(logger, "OnTokenValidated", DataProcessingStatus.Success, "Token validated successfully.");

                            return Task.CompletedTask;
                        }
                    };
                });
            */

            /*
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;


            })
                .AddMicrosoftIdentityWebApp(options =>
            {
                // Bind configuration settings from appsettings.json
               // builder.Configuration.GetSection("AzureAd");

                options.Instance = adss.Instance;
                options.Domain = adss.Domain;
                options.TenantId = adss.TenantId;
                options.ClientId = adss.ClientId;
                options.ClientSecret = adss.ClientCredentials.FirstOrDefault(c => !string.IsNullOrEmpty(c.ClientSecret))?.ClientSecret;
                options.CallbackPath = adss.CallbackPath; // Adjust as necessary
                //options.ReturnUrlParameter = "/Red/Inside";

                // Token validation parameters
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                  //  IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-signing-key")),
                    ValidateIssuer = true,
                    ValidIssuer = $"https://login.microsoftonline.com/{adss.TenantId}/v2.0", // Ensure this matches your configuration
                    ValidateAudience = true,
                    ValidAudience = adss.ClientId, // Use your app's Client ID as the audience
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5) // Optional: Set clock skew for token expiration checks
                };


                // Specify the scopes you want to request
                options.Scope.Add("openid"); // Required for OpenID Connect
                options.Scope.Add("profile"); // To get user profile information
                options.Scope.Add("email"); // To get the user's email address
                                            // Add any additional scopes as needed



                // Provide a post-login redirect
                options.Events = new OpenIdConnectEvents
                {
                    OnTokenValidated = context =>
                    {
                        var audience = context.Principal.Claims
                                                  .FirstOrDefault(c => c.Type == "aud")?.Value;

                        if (audience != adss.ClientId) // Compare to your expected audience
                        {
                            // Log, handle unauthorized access
                            LoggingHelper.LogDataProcessingStatusServiceWork(logger, "OnTokenValidated",
                                DataProcessingStatus.Failure, "Invalid audience detected.");

                            context.Response.Redirect("/Privacy"); 
                            context.HandleResponse(); // Prevent further processing
                        }
                        else
                        {
                            // Proceed with normal flow
                            LoggingHelper.LogDataProcessingStatusServiceWork(logger, "OnTokenValidated",
                                DataProcessingStatus.Success, "Valid audience");
                            context.Response.Redirect("/Index");
                            context.HandleResponse();
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, DataProcessingStatus.Failure, $"OnAuthenticationFailed {context.Exception.ToString()}");
                        return Task.CompletedTask;
                    }
                };
            });

            */

            /*
             * We have a problem with application layer client certificate requirements.
             * Currently, the application requires a client cert from Azure Web App config.
             * We need to truncate this troubleshooting for now to hit a project goal.
             * 
             
            builder.Services
                    .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                    .AddCertificate(options =>
                    {
                        builder.Configuration.GetSection("AzureAd");

                        // Only allow chained certs, no self signed
                        options.AllowedCertificateTypes = CertificateTypes.Chained;
                        // Don't perform the check if a certificate has been revoked
                        options.RevocationMode = X509RevocationMode.NoCheck;
                        options.Events = new CertificateAuthenticationEvents()
                        {
                            OnAuthenticationFailed = context =>
                            {
                                LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, "", DataProcessingStatus.Failure, "Auth failed at Client Cert");
                                return Task.CompletedTask;
                            },
                            OnCertificateValidated = context =>
                            {
                                LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, "", DataProcessingStatus.Success, "Auth succeeded at Client Cert");
                                return Task.CompletedTask;
                            }
                        };
                    });

            */

            #endregion
            #region Critical Edits Region


            // Add services to the container.
            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

            // CRITICAL EDITS FOLLOW


            builder.Services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy.
                // Commented out line below under professor advice - critical for operational success.
                // options.FallbackPolicy = options.DefaultPolicy;
            });

            builder.Services.AddRazorPages(

                options => {

                    options.Conventions.AuthorizeFolder("/Red");
                    options.Conventions.AuthorizeFolder("/Red/SearchBy");
                    options.Conventions.AuthorizeFolder("/Red/FileUploads");
                    options.Conventions.AuthorizeFolder("/Red/DataEntry");
                    options.Conventions.AuthorizeFolder("/Red/RiskRegistry");

                    // options.Conventions.AllowAnonymousToPage("/Index");
                    options.Conventions.AllowAnonymousToPage("/Privacy");
                    options.Conventions.AllowAnonymousToPage("/Error");
                    options.Conventions.AllowAnonymousToPage("/About");

                }
                )
                .AddMicrosoftIdentityUI();


            // The AzureAD values from the service, above,
            // are not getting stitched in properly here.
            // Adjust. 




            /*

                    builder.Services.AddAuthorizationBuilder()
                                    .AddPolicy("DefaultPolicy", policy =>
                            policy.RequireAuthenticatedUser());

                    */

            LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, DataProcessingStatus.Info, "Ending Critical Edits section for AD B2C and Entra ID External");

            // CRITICAL EDITS ABOVE

            #endregion



            #region Key Vault Operations
            // Retrieve Key Vault Values

            LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, DataProcessingStatus.Info, "Beginning Key Vault Operations");
            try
            {
                KeyVaultSettings kvs = builder.Configuration.GetSection("AzureKeyVault").Get<KeyVaultSettings>()! ?? throw new InvalidOperationException("Values for KeyVaultSettings not found.")!;

                AzureADSettings aads = builder.Configuration.GetSection("AzureAD").Get<AzureADSettings>()! ?? throw new InvalidOperationException("Values for AzureADSettings not found.")!;

                if (string.IsNullOrEmpty(aads.ClientId))
                {
                    throw new InvalidOperationException("ClientId must be provided.");
                }

                var clientSecretElement = aads.ClientCredentials.FirstOrDefault(cc => cc.SourceType == "ClientSecret");
                string clientSecretValue = clientSecretElement?.ClientSecret! ?? String.Empty;

                var akvoLogger = loggerFactory.CreateLogger<AzureKeyVaultCertificateOperations>();
                var akvo = new AzureKeyVaultCertificateOperations(akvoLogger);

                var serverCert = await akvo.GetCertificateFromKeyVault(aads.TenantId, aads.ClientId, kvs.KeyVaultURL, kvs.KeyVaultSecret, kvs.KeyVaultPassName);

                if (serverCert == null)
                {
                    throw new Exception("Server certificate not retrieved from Key Vault.");
                }
                else
                {

                    LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, "", DataProcessingStatus.Info, "serverCert is not null");

                }

                // Wire Kestrel to use Server certs and require Client Certs
                builder.WebHost.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
                    {
                        if (environment.IsDevelopment())
                        {
                            //httpsOptions.ServerCertificate = serverCert;
                            //httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                        }
                        httpsOptions.ServerCertificate = serverCert;
                        //httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                    });
                });


                LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, DataProcessingStatus.Info, "Adding Certificate Authentication section.");







            }
            catch (Exception ex)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, "", DataProcessingStatus.Exception, ex.Message);
            }

            #endregion

            #region Provide various Settings Services

            builder.Services.AddSingleton<ILoggerFactory, LoggerFactory>();
            builder.Services.AddDistributedMemoryCache();

            // Create a KeyVaultService for use elsewhere.

            KeyVaultSettings kvss = builder.Configuration.GetSection("AzureKeyVault").Get<KeyVaultSettings>()! ?? throw new InvalidOperationException("Values for KeyVaultSettings not found.")!;

            builder.Services.AddSingleton<IKeyVaultSettingsService>(new KeyVaultSettingsService(kvss));

            // Create a AzureADService for use elsewhere.
            /*
             * Temporarily moved higher in the sequence to help an options in auth.
             * 
            AzureADSettings adss = builder.Configuration.GetSection("AzureAD").Get<AzureADSettings>()! ?? throw new InvalidOperationException("Values for AzureADSettings not found.")!;

            LoggingHelper.LogDataProcessingStatusServiceWork(logger, "Program.Main", string.Empty, DataProcessingStatus.Info, $"AzureADSettings ClientID check: {adss.ClientId}");


            builder.Services.AddSingleton<IAzureADSettingsService>(new AzureADSettingsService(adss));
            */

            /*
            // Register IAzureADSettingsService with dependency injection
            builder.Services.AddSingleton<IAzureADSettingsService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<AzureADSettings>>().Value;
                if (settings == null)
                {
                    throw new InvalidOperationException("Values for AzureADSettings not found.");
                }
                return new AzureADSettingsService(settings);
            });
            */

            // Create a NonceEncryptionSettingsService for use elsewhere.

            NonceEncryptionSettings ness = builder.Configuration.GetSection("NonceEncryption").Get<NonceEncryptionSettings>()! ?? throw new InvalidOperationException("Values for NonceEncryptionSettings not found.")!;

            builder.Services.AddSingleton<INonceEncryptionSettingsService>(new NonceEncryptionSettingsService(ness));




            // Create a NonceCatalogService for use elsewhere.;


            builder.Services.AddSingleton<IAzureKeyVaultCertificateOperations, AzureKeyVaultCertificateOperations>();
            builder.Services.AddSingleton<IAzureKeyVaultOperationsService, AzureKeyVaultOperationsService>();
            builder.Services.AddSingleton<INonceRefresherService, NonceRefresherService>();
            builder.Services.AddSingleton<INonceCatalogService, NonceCatalogService>();

            // Breaks right here.

            #endregion

            #region BlobSettings

            // BlobConnectionString
            BlobSettings? _blobSettings = builder.Configuration.GetSection("BlobSettings").Get<BlobSettings>()!;

            if (_blobSettings.BlobConnectionString != null)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(logger, "Program.Main", string.Empty, DataProcessingStatus.Info, $"BlobSettings 5 character sample: {_blobSettings.BlobConnectionString[..5]}");

                LoggingHelper.LogDataProcessingStatusServiceWork(logger, "Program.Main", string.Empty, DataProcessingStatus.Info, $"BlobSettings MaxAttachments: {_blobSettings.MaxAttachments}");
            }


            if (_blobSettings is null
                || _blobSettings.BlobConnectionString == null
                || _blobSettings.BlobConnectionString.Length == 0)
            {

                if (_blobSettings == null)
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(logger, "Program.Main", string.Empty, DataProcessingStatus.Failure, "BlobSettings not read into program.");

                }
                else
                {
                    if (_blobSettings.BlobConnectionString == null)
                    {
                        LoggingHelper.LogDataProcessingStatusServiceWork(logger, "Program.Main", string.Empty, DataProcessingStatus.Error, "BlobConnectionString is null.");
                    }
                    else
                    {

                        LoggingHelper.LogDataProcessingStatusServiceWork(logger, "Program.Main", string.Empty, DataProcessingStatus.Error, $"BlobSettings set to {_blobSettings.BlobConnectionString}");
                    }
                }
                throw new InvalidOperationException("BlobSettings validation failed. Check the logs for details.");
            }
            else
            {

                LoggingHelper.LogDataProcessingStatusServiceWork(logger, "Program.Main", string.Empty, DataProcessingStatus.Success, $"BlobSettings set.");
            }

            builder.Services.AddSingleton(_blobSettings);
            builder.Services.AddScoped<IBlobSettingsService, BlobSettingsService>();

            #endregion

            #region Connect to database

            try
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, "", DataProcessingStatus.Info, "Beginning CosmosDb configuration and context");

                // Connect application EF to Azure DB
                CosmosDbSettings? cosmosSettings = builder.Configuration.GetSection("CosmosDb").Get<CosmosDbSettings>()! ?? throw new InvalidOperationException("Connection string for CosmosDB not found.")!;

                LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, "", DataProcessingStatus.Info, $"Sample last 5 chars from connection string:  {cosmosSettings.CosmosConnectionString[^5..]}");

                // Ensure we can talk to the database

                var client = new CosmosClient(cosmosSettings.CosmosConnectionString);

                LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, "", DataProcessingStatus.Info, $"Cosmos Client Endpoint:  {client.Endpoint.ToString}");

                Database database = client.GetDatabase(cosmosSettings.DatabaseName);
                database = await database.ReadAsync();

                LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, "", DataProcessingStatus.Info, $"Database Client:  {database.Client.ToString}");


                builder.Services.AddSingleton<ICosmosDbSettingsService>(new CosmosDbSettingsService(cosmosSettings));



                // Be able to instantiate the Cosmos Client one time.

                builder.Services.AddSingleton<CosmosClient>((serviceProvider) =>
                {
                    CosmosClient client = new(cosmosSettings.CosmosConnectionString);
                    return client;
                });

                // Be able to instantiate the Database (inside CosmosDbSettings) one time.

                builder.Services.AddSingleton<CosmosDbService>(provider =>
                {
                    var client = provider.GetRequiredService<CosmosClient>();
                    var databaseName = provider.GetRequiredService<CosmosDbSettings>().DatabaseName;
                    return new CosmosDbService(client, databaseName);
                });

                builder.Services.AddDbContext<RedCosmosDBContext>((serviceprovider, options) => options.UseCosmos(
                    cosmosSettings.AccountEndpoint,
                    cosmosSettings.AccountKey,
                    cosmosSettings.DatabaseName
                    ));

                // show me the context.
                var serviceProvider = builder.Services.BuildServiceProvider();

                LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, "", DataProcessingStatus.Info, $"ServiceProvider check: {serviceProvider.GetHashCode()}");

                var services = serviceProvider.GetServices<RedCosmosDBContext>();

                LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, "", DataProcessingStatus.Info, $"Checking for RedCosmosDBContext: {services.First<RedCosmosDBContext>().ContextId}");

            }
            catch (Exception ex)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, "", DataProcessingStatus.Exception, ex.Message);
            }
            finally
            {

                LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, "", DataProcessingStatus.Info, "Ending CosmosDb configuration and context");
            }


            #endregion



            #region Build and Run


            try
            {

                LoggingHelper.LogDataProcessingStatusServiceWork(logger, caller, "", DataProcessingStatus.Info, "Beginning app builder.Build()");

                var app = builder.Build();

                // Now we can access services built above.  They are available after builder.Build()


                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Error");

                }
                else
                {
                    app.UseExceptionHandler("/Error");
                    app.UseHsts(); // Adds HTTP Strict Transport Security (HSTS) headers
                }

                app.UseHttpsRedirection();

                app.UseRouting();
                app.UseSession();

                app.UseAuthentication();

                app.UseAuthorization();

                #region Set Security Headers in Kestrel

                // REF:  https://cheatsheetseries.owasp.org/cheatsheets/HTTP_Headers_Cheat_Sheet.html

                // Generate a Nonce and log it.
                // working
                string CSPNonce = string.Empty;
                await app.Services.GetRequiredService<INonceRefresherService>().RefreshNonceAsync();

                app.UseMiddleware<NonceMiddleware>();
                app.UseMiddleware<LoggingMiddleware>();

                app.Use(async (context, next) =>
                {
                    // With this call here, CSPNonce is changing on the pages and in the CSP.
                    // It looke like the middleware is critical for that flow.
                    // The middleware will load the context that the page needs.
                    // There is a refresh inside that process which is doing the advancing.
                    // Thus we need to load into CSP afterwards, here.
                    CSPNonce = app.Services.GetRequiredService<INonceCatalogService>().GetANonce("CSPNonce");

                    // Set a CSP in order to advise browsers of where to get their data from us.
                    // REF: https://cheatsheetseries.owasp.org/cheatsheets/Content_Security_Policy_Cheat_Sheet.html

                    StringBuilder CSPText = new StringBuilder();
                    CSPText.Append("default-src 'none'; script-src https: 'self' https://www.physicalsecurityatlas.com  'nonce-");
                    CSPText.Append(CSPNonce);
                    CSPText.Append("'; connect-src 'self'; img-src 'self' data:; style-src 'self' https://experience.arcgis.com https://harvard-cga.maps.arcgis.com; frame-src https://experience.arcgis.com https://harvard-cga.maps.arcgis.com; frame-ancestors 'self'; form-action 'self';");

                    // Temporarily turn off CSP because of heatmap needs
                    context.Response.Headers.Append("Content-Security-Policy", CSPText.ToString());

                    // Prevent clickjacking or framing our pages to use as bait.
                    context.Response.Headers.Append("X-Frame-Options", "DENY");

                    // Tell the browser to defend itself against XSS
                    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

                    // Prevent MIME-sniffing
                    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

                    // FOUND IT.  THIS WAS CAUSING DENIALS 403
                    // Control how much referrer information should be passed.
                    // context.Response.Headers.Append("Referrer-Policy", "no-referrer-when-downgrade");

                    // Describe the content
                    context.Response.Headers.Append("Content-Type", "text/html; charset=UTF-8");

                    // Limit how cookies are used.
                    context.Response.Headers.Append("Set-Cookie", "path=/; Secure; HttpOnly; SameSite=Strict");

                    // Enforce HTTPS
                    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; include subdomains");

                    // Access-Control-Allow-Origin : use default same-origin policy
                    context.Response.Headers.Append("Access-Control-Allow-Origin", "experience.arcgis.com");

                    // Protect against SPECTRE
                    context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin");

                    // Prevent loading any cross-origin sources
                    // context.Response.Headers.Append("Cross-Origin-Embedder-Policy", "require-corp");

                    // Protect against SPECTRE
                    context.Response.Headers.Append("Cross-Origin-Resource-Policy", "same-site");

                    // Limit the use of other systems features like camera and microphone
                    // Limit the use of FLoC
                    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), camera=(), microphone=(), interest-cohort=()");

                    // Limit fingerprinting by providing a generic description of the server
                    context.Response.Headers.Remove("Server");
                    context.Response.Headers.Append("Server", "webserver");
                    context.Response.Headers.Remove("X-Powered-By");
                    context.Response.Headers.Remove("X-AspNetMvc-Version");


                    // Require revalidation
                    context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                    context.Response.Headers.Append("Pragma", "no-cache");
                    context.Response.Headers.Append("Expires", "0");

                    await next.Invoke();


                });


                #endregion




                app.MapStaticAssets();

                app.MapRazorPages()
                   .WithStaticAssets();

                app.MapControllers();

                app.Run();

            }
            catch (Exception ex)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(logger, string.Empty, DataProcessingStatus.Exception, ex.Message);
            }
            finally
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(logger, string.Empty, DataProcessingStatus.Info, "Ending app builder.Build() and app.Run()");
            }


        }
    }
}

#endregion