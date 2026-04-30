using WebAppExperimental26.Models.Main_Objects;

namespace REDRFID.Services
{

    public class NonceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly INonceCatalogService _nonceCatalogService;
        private readonly ILogger<NonceMiddleware> _logger;
        private readonly INonceRefresherService _nonceRefresherService;
        private readonly IAzureADSettingsService _azureADSettingsService;

        public NonceMiddleware(RequestDelegate next, INonceCatalogService nonceCatalogService, ILogger<NonceMiddleware> logger, INonceRefresherService nonceRefresherService, IAzureADSettingsService azureADSettingsService)
        {
            _logger = logger;
            _next = next;
            _nonceCatalogService = nonceCatalogService;
            _nonceRefresherService = nonceRefresherService;
            _azureADSettingsService = azureADSettingsService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string caller = "NonceMiddleware.InvokeAsync()";
            // Generate the nonce
            await _nonceRefresherService.RefreshNonceAsync();
            var nonce = _nonceCatalogService.GetANonce("CSPNonce");

            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, "", DataProcessingStatus.Info, $"Nonce: {nonce}");

            // Store the nonce in the HttpContext
            context.Items["Nonce"] = nonce;

            await _next(context);
        }
    }
}
