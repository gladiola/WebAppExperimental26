using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using REDRFID.Data;
using REDRFID.Models.Main_Objects;
using REDRFID.Models.Storage;
using REDRFID.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REDRFID.Pages.Red.DataEntry
{
    public class DetailsModel : PageModel
    {

        private readonly RedCosmosDBContext _context;

        private readonly ILogger<DetailsModel> _logger;

        private ICosmosDbSettingsService _cosmosDbSettingsService;

        public DetailsModel(RedCosmosDBContext context, ILogger<DetailsModel> logger, ICosmosDbSettingsService cosmosDbSettingsService)
        {
            _context = context;
            _logger = logger;
            _cosmosDbSettingsService = cosmosDbSettingsService;
        }

        [BindProperty]
        public RedIdRecord RedIdRecord { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            string caller = "/Red/DataEntry/DetailsModel.OnGetAsync()";
            if (id == null)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"id parameter was null");
                return NotFound();
            }

            // Convert the integer to a string to go better with the query below.
            string idOut = Convert.ToString(id) ?? string.Empty;

            try
            {
                using (_context)
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Using context {_context.ContextId}");

                    Database database = _context.Database.GetCosmosClient().GetDatabase(_cosmosDbSettingsService.GetSettings().DatabaseName);

                    database = await database.ReadAsync();
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Got database:\t{database.Id}");

                    Container container = _context.Database.GetCosmosClient().GetContainer(_cosmosDbSettingsService.GetSettings().DatabaseName, _cosmosDbSettingsService.GetSettings().ContainerName);

                    container = await container.ReadContainerAsync();
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Got container:\t{container.Id}");

                    var query = new QueryDefinition(query: "SELECT * FROM RedRecords c WHERE c.id = @id").WithParameter("@id", idOut);

                    using FeedIterator<RedIdRecord> feed = container.GetItemQueryIterator<RedIdRecord>(queryDefinition: query);

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Feed status after query for SELECT {idOut}:  feed: {feed.ToString()}");

                    while (feed.HasMoreResults)
                    {
                        FeedResponse<RedIdRecord> response = await feed.ReadNextAsync();
                        var redidrecord = response.FirstOrDefault<RedIdRecord>();
                        if (redidrecord == null)
                        {
                            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Failure, $"Item to delete was not found.  id: {idOut}");
                        }
                        else
                        {
                            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Success, $"Item to delete was found.  id: {idOut}");
                            RedIdRecord = redidrecord;
                            return Page();
                        }
                    }

                    if (RedIdRecord == null)
                    {
                        LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Item to delete was not found.  id: {idOut}");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Exception, ex.Message);
            }

            await Task.Delay(1);

            return NotFound();
    
        }
    }
}
