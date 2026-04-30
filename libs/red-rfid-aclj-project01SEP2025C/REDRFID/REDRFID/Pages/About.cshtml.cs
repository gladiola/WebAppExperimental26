using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using REDRFID.Services;
using System.Diagnostics;

namespace REDRFID.Pages
{
    /// <summary>
    /// Page to hold public About content.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AboutModel : PageModel
    {
        private readonly ILogger<AboutModel> _logger;
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public AboutModel(ILogger<AboutModel> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// React to HTTP GET
        /// </summary>
        /// <returns>Task to satisfy need for an internal await.</returns>
        public async Task OnGet()
        {
            // Log who is calling
            string caller = "AboutModel.OnGet()";
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            LoggingHelper.LogTraceIdentifier(_logger, RequestId);
            string noteRequestId = LoggingHelper.LogRequestReturnId(Request, _logger, caller);
            await LoggingHelper.LogUserActivity(User, _logger, caller, noteRequestId);
        }
    }
}
