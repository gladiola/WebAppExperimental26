using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using REDRFID.Models.Main_Objects;
using REDRFID.Pages.Red.MapWork;
using REDRFID.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace REDRFID.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly INonceCatalogService _nonceCatalogService;
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string Nonce {  get; private set; }


        public IndexModel(ILogger<IndexModel> logger, INonceCatalogService nonceCatalogService)
        {
            _logger = logger;
            _nonceCatalogService = nonceCatalogService;
        }




        public async Task OnGet()
        {
            Nonce = _nonceCatalogService.GetANonce("CSPNonce");

            // Log who is calling
            string caller = "IndexModel.OnGet()";
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            LoggingHelper.LogTraceIdentifier(_logger, RequestId);
            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, RequestId, DataProcessingStatus.Info, $"Nonce retrieved: {Nonce}");
            string noteRequestId = LoggingHelper.LogRequestReturnId(Request, _logger, caller);
            await LoggingHelper.LogUserActivity(User, _logger, caller, noteRequestId);

        }

        public IActionResult OnGetDownloadExcel()
        {

            string caller = "IndexModel.DownloadFile()";
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            LoggingHelper.LogTraceIdentifier(_logger, RequestId);
            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, RequestId, DataProcessingStatus.Info, $"Download file to user.");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads", "RFID-Risk-Registry-01.xlsx"); 


            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, RequestId, DataProcessingStatus.Info, $"Download file from {filePath} to user.");

            // Get the file name
            var fileName = Path.GetFileName(filePath);
            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, RequestId, DataProcessingStatus.Info, $"Download {fileName} to user.");

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, RequestId, DataProcessingStatus.Error, $"{filePath} did not exist.  Returning 404");
                return NotFound(); // Return 404 if the file is not found
            }

            // Return the file for download
            return File(System.IO.File.ReadAllBytes(filePath), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
