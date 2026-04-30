using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using REDRFID.Data;
using REDRFID.Models.Main_Objects;
using REDRFID.Models.Storage;
using REDRFID.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace REDRFID.Pages.Red.DataEntry
{
    public class CreateModel : PageModel
    {
        private readonly RedCosmosDBContext _context;

        private readonly ILogger<CreateModel> _logger;

        private ICosmosDbSettingsService _cosmosDbSettingsService;

        public string? RequestId { get; set; }

        public CreateModel(RedCosmosDBContext context, ILogger<CreateModel> logger, ICosmosDbSettingsService cosmosDbSettingsService)
        {
            _context = context;
            _logger = logger;
            _cosmosDbSettingsService = cosmosDbSettingsService;
        }


        public IActionResult OnGet()
        {
            string caller = "/Red/DataEntry/CreateModel.OnGet()";

            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, "OnGet() only returns Page().");

            return Page();
        }

        [BindProperty]
        public RedIdRecord RedIdRecord { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {

            // Log who is calling
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            string caller = "/Red/DataEntry/CreateModel.OnPostAsync()";
            LoggingHelper.LogTraceIdentifier(_logger, RequestId);
            string noteRequestId = LoggingHelper.LogRequestReturnId(Request, _logger, caller);
            await LoggingHelper.LogUserActivity(User, _logger, caller, noteRequestId);

            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, "OnPostAsync() function begins.");


            if (!ModelState.IsValid)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Error, "Model state was not valid.");
                return Page();
            }

            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, "ModelState has been determined to be valid.");


            try
            {
                using (_context)
                {
                    Database database = _context.Database.GetCosmosClient().GetDatabase(_cosmosDbSettingsService.GetSettings().DatabaseName);

                    database = await database.ReadAsync();
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Got database:\t{database.Id}");

                    Container container = _context.Database.GetCosmosClient().GetContainer(_cosmosDbSettingsService.GetSettings().DatabaseName, _cosmosDbSettingsService.GetSettings().ContainerName);

                    container = await container.ReadContainerAsync();
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Got container:\t{container.Id}");


                    var received = await _context.RedRecords.ToListAsync();

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Received: {received.Count()}");

                    // Add server-provided values
                    RedIdRecord.Id = Random.Shared.Next(10, int.MaxValue);
                    RedIdRecord.UploadedUser = HttpContext.TraceIdentifier.ToString();
                    RedIdRecord.UserID = HttpContext.TraceIdentifier.ToString();
                    RedIdRecord.IdString = Guid.NewGuid().ToString();
                    RedIdRecord._partitionKey = "85F05E2D1492C89F3EC1052A";

                    await _context.AddAsync(RedIdRecord);

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, "after async add record");

                    var changedItems = await _context.SaveChangesAsync();

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"after async save changes reported {changedItems.ToString()} state entries written to database");

                    

                }
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Exception, ex.Message);
            }

            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, "Function work completed.  Redirecting to page.");

            return RedirectToPage("./Index");
        }
    }
}
