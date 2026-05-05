using Microsoft.AspNetCore.Authentication;
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
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using WebAppExperimental26.Models.Main_Objects;
using WebAppExperimental26.Models.Settings;
using WebAppExperimental26.Extensions;
using WebAppExperimental26.Services;

namespace WebAppExperimental26
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var environment = builder.Environment;

            // Logging
            var loggerFactory = LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
            });

            var logger = loggerFactory.CreateLogger("Program");
            logger.LogInformation("=== Application Starting - {Environment} ===", environment.EnvironmentName);

            // Initialize PII hashing for log output (security task 16).
            // Supply a stable 32-byte base64 key via Logging:PiiHmacKey (User Secrets / Key Vault).
            // If absent or invalid, a random key is used for this process lifetime.
            var piiHmacKeyBase64 = builder.Configuration["Logging:PiiHmacKey"];
            byte[]? piiHmacKey = null;
            if (!string.IsNullOrWhiteSpace(piiHmacKeyBase64))
            {
                try
                {
                    var decoded = Convert.FromBase64String(piiHmacKeyBase64);
                    if (decoded.Length == 32)
                    {
                        piiHmacKey = decoded;
                    }
                    else
                    {
                        logger.LogWarning("Logging:PiiHmacKey must be exactly 32 bytes when base64-decoded (got {Length} bytes); a random key will be used for this session.", decoded.Length);
                    }
                }
                catch (FormatException)
                {
                    logger.LogWarning("Logging:PiiHmacKey is not valid base64; a random key will be used for this session.");
                }
            }
            LoggingHelper.Initialize(piiHmacKey);

            // Load feature flags
            builder.Services.AddFeatureFlags(builder.Configuration);
            var featureFlags = builder.Configuration.GetSection("FeatureFlags").Get<FeatureFlags>() ?? new FeatureFlags();

            logger.LogInformation("Feature Flags Loaded: AzureAd={AzureAd}, CosmosDb={CosmosDb}, BlobStorage={BlobStorage}",
                featureFlags.EnableAzureAd, featureFlags.EnableCosmosDb, featureFlags.EnableBlobStorage);

            // Core services
            builder.Services.AddSingleton<ILoggerFactory, LoggerFactory>();

            // Phase 1: Basic Infrastructure
            builder.Services.AddSessionConfiguration(logger, featureFlags.EnableSession);
            builder.Services.AddLocalizationConfiguration(logger, featureFlags.EnableLocalization);

            // Phase 2: Authentication & Authorization
            builder.Services.AddAzureAdAuthentication(
                builder.Configuration,
                logger,
                featureFlags.EnableAzureAd);

            // Phase 2.5: mTLS Client Certificate Authentication (if enabled)
            if (featureFlags.EnableMtls)
            {
                builder.Services.AddMtlsAuthentication(
                    builder.Configuration,
                    logger,
                    featureFlags.EnableMtls);
            }

            builder.Services.AddRazorPagesConfiguration(
                logger,
                featureFlags.EnableAuthorization);

            // Phase 3: Azure Services (Advanced)
            if (featureFlags.EnableKeyVault)
            {
                await builder.Services.AddKeyVaultServicesAsync(
                    builder.Configuration,
                    loggerFactory,
                    environment,
                    builder);
            }

            // Phase 2: Security (Nonce & CSP)
            builder.Services.AddNonceServices(
                builder.Configuration,
                logger,
                featureFlags.EnableNonceServices);

            // Phase 3: Data Storage (Optional)
            if (featureFlags.EnableBlobStorage)
            {
                builder.Services.AddBlobStorageServices(
                    builder.Configuration,
                    logger,
                    featureFlags.EnableBlobStorage);
            }

            if (featureFlags.EnableCosmosDb)
            {
                await builder.Services.AddCosmosDbServicesAsync(
                    builder.Configuration,
                    logger,
                    featureFlags.EnableCosmosDb);
            }

            logger.LogInformation("=== Service Configuration Complete ===");

            // Build app
            var app = builder.Build();

            // Configure HTTP pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            if (featureFlags.EnableSession)
            {
                app.UseSession();
            }

            if (featureFlags.EnableAzureAd)
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            // Security Middleware
            if (featureFlags.EnableCSP && featureFlags.EnableNonceServices)
            {
                await app.UseNonceAndSecurityHeadersAsync(logger, enabled: true);
            }

            if (featureFlags.EnableSecurityHeaders)
            {
                app.UseStandardSecurityHeaders(logger, enabled: true);
            }

            // Map endpoints
            app.MapStaticAssets();
            app.MapRazorPages().WithStaticAssets();
            app.MapControllers();

            logger.LogInformation("=== Application Ready - Starting Server ===");
            app.Run();
        }
    }
}