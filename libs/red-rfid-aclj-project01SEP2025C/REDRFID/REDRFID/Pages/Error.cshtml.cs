using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using REDRFID.Services;
using System.Diagnostics;

namespace REDRFID.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            // Log who is calling
            string caller = "ErrorModel.OnGet()";
            LoggingHelper.LogTraceIdentifier(_logger, RequestId);
            string noteRequestId = LoggingHelper.LogRequestReturnId(Request, _logger, caller);
            await LoggingHelper.LogUserActivity(User, _logger, caller, noteRequestId);

        }
    }

}
