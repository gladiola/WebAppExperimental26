using Microsoft.Extensions.Logging;
using WebAppExperimental26.AzureKeyVaultOperations;
using WebAppExperimental26.Models.Main_Objects;
using WebAppExperimental26.Models.Settings;

namespace WebAppExperimental26.Services
{
    public interface INonceRefresherService {
        public Task<string> RefreshNonceAsync();
    }

    public class NonceRefresherService : INonceRefresherService
    {

        private readonly ILogger<NonceRefresherService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IKeyVaultSettingsService _keyVaultSettingsService;
        private readonly INonceCatalogService _nonceCatalogService;
        private readonly INonceEncryptionSettingsService _nonceEncryptionSettingsService;
        private readonly IAzureADSettingsService _azureADSettingsService;
        private readonly IAzureKeyVaultOperationsService _azureKeyVaultOperationsService;

        public NonceRefresherService(ILogger<NonceRefresherService> logger, ILoggerFactory loggerFactory, IKeyVaultSettingsService keyVaultSettingsService, INonceEncryptionSettingsService nonceEncryptionSettingsService, IAzureADSettingsService azureADSettingsService, INonceCatalogService nonceCatalogService, IAzureKeyVaultOperationsService azureKeyVaultOperationsService) { 
        
            _logger = logger;
            _loggerFactory = loggerFactory;
            _keyVaultSettingsService = keyVaultSettingsService;
            _nonceCatalogService = nonceCatalogService;
            _nonceEncryptionSettingsService = nonceEncryptionSettingsService;
            _azureADSettingsService = azureADSettingsService;
            _azureKeyVaultOperationsService = azureKeyVaultOperationsService;

        }


        public async Task<string> RefreshNonceAsync()
        {
            string caller = "NonceRefresherService.RefreshNonce()";
            string CSPNonce = string.Empty;

            try
            {
                // Generate a fresh cryptographically random nonce using RandomNumberGenerator.
                // No Key Vault IV/key fetch is required — see Nonce.cs for the security rationale.
                var nonceLogger = _loggerFactory.CreateLogger<Nonce>();
                Nonce nonce = new(nonceLogger);
                CSPNonce = nonce.GetNonceAsString();

                if (string.IsNullOrEmpty(CSPNonce))
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, "", DataProcessingStatus.Failure, $"Did not generate Nonce.");
                }
                else
                {
                    // Log only success status — never log the nonce value (see Critical #2).
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, "", DataProcessingStatus.Success, "Nonce generated successfully.");
                }

                _nonceCatalogService.AddANonce("CSPNonce", nonce);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, "", DataProcessingStatus.Exception, ex.Message);
            }

            return CSPNonce;
        }
    }
}
