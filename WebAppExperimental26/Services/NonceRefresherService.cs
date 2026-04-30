using Microsoft.Extensions.Logging;
using WebAppExperimental26.AzureKeyVaultOperations;
using WebAppExperimental26.Models.Main_Objects;
using WebAppExperimental26.Models.Settings;
using WebAppExperimental26.Services;

namespace REDRFID.Services
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
                

                AzureADSettings aads = _azureADSettingsService.GetSettings();

                // breaks right here.

                if (aads.ClientCredentials == null || !aads.ClientCredentials.Any())
                {
                    throw new InvalidOperationException("ClientCredentials collection is empty or null");
                }
                else {

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, "", DataProcessingStatus.Success, $"Sample from client secret:  {aads.ClientCredentials.ToString()}");
                }

                    var clientSecretElement = aads.ClientCredentials.FirstOrDefault(cc => cc.SourceType == "ApplicationSecret");

                string clientSecretValue = clientSecretElement?.ClientSecret! ?? String.Empty;

                NonceEncryptionSettings nes = _nonceEncryptionSettingsService.GetSettings();

                // We have a problem here.  
                // The values for IV and EncKey results are not as expected.
                // When they are applied in Nonce creation, this will throw an exception.

                var fetchIV = await _azureKeyVaultOperationsService.FetchSecretIVSecret();

                var fetchEncKey = await _azureKeyVaultOperationsService.FetchSecretNonceKeySecret();

                var nonceLogger = _loggerFactory.CreateLogger<Nonce>();
                Nonce nonce = new(nonceLogger, fetchIV, fetchEncKey);
                CSPNonce = nonce.GetNonceAsString();

                if (string.IsNullOrEmpty(CSPNonce))
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, "", DataProcessingStatus.Failure, $"Did not generate Nonce.");
                }
                else
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, "", DataProcessingStatus.Success, $"Generated Nonce: {CSPNonce}");
                }
                
                _nonceCatalogService.AddANonce("CSPNonce", nonce);
                

            }
            catch (Exception ex)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, "", DataProcessingStatus.Exception, ex.Message);

            }


            return CSPNonce;
        }
    }
}
