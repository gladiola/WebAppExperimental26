using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using REDRFID.Services;
using System.Diagnostics;

namespace REDRFID.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGet()
        {
            // Log who is calling
            string caller = "PrivacyModel.OnGet()";
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            LoggingHelper.LogTraceIdentifier(_logger, RequestId);
            string noteRequestId = LoggingHelper.LogRequestReturnId(Request, _logger, caller);
            await LoggingHelper.LogUserActivity(User, _logger, caller, noteRequestId);
        }
    }

}
