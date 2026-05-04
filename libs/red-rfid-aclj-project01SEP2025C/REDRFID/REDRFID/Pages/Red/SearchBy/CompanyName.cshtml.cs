using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using REDRFID.Data;
using REDRFID.Models.Main_Objects;
using REDRFID.Models.Storage;
using REDRFID.Models.ResultObjects;
using REDRFID.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace REDRFID.Pages.Red.SearchBy
{
    public class CompanyNameModel : PageModel
    {
        private readonly RedCosmosDBContext _context;

        private readonly ILogger<CompanyNameModel> _logger;

        private ICosmosDbSettingsService _cosmosDbSettingsService;

        public CompanyNameModel(RedCosmosDBContext context, ILogger<CompanyNameModel> logger, ICosmosDbSettingsService cosmosDbSettingsService)
        {
            _context = context;
            _logger = logger;
            _cosmosDbSettingsService = cosmosDbSettingsService;
        }

        [BindProperty]
        public IList<RedIdRecord> RedIdRecord { get;set; } = new List<RedIdRecord>();

        [BindProperty]
        public string CompanyName { get; set; }

        public int ListLength { get; set; } = 0;


        public async Task<IActionResult> OnGetAsync()
        {
            string caller = "/Red/SearchBy/CompanyNameModel.OnGetAsync()";
            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"OnGetAsync was called.  No data processing needed.");

            await Task.Delay(1);

            return Page();


        }
        


        public async Task<IActionResult> OnPostAsync()
        {
            string caller = "/Red/DataEntry/CompanyNameModel.OnPostAsync()";

            if (!ModelState.IsValid) {
                return Page();
            }

            if (String.IsNullOrEmpty(CompanyName))
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"search parameter was null");
                return NotFound();
            }

            // Convert the integer to a string to go better with the query below.
            string idOut = Convert.ToString(CompanyName);

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

                    var query = new QueryDefinition(query: "SELECT * FROM RedRecords c WHERE c.CompanyName = @CompanyName").WithParameter("@CompanyName", idOut);

                    using FeedIterator<RedIdRecord> feed = container.GetItemQueryIterator<RedIdRecord>(queryDefinition: query);

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Feed status after query for SELECT {idOut}:  feed: {feed.ToString()}");

                    List<RedIdRecord> items = new();

                    while (feed.HasMoreResults)
                    {
                        FeedResponse<RedIdRecord> response = await feed.ReadNextAsync();


                        foreach (RedIdRecord item in response)
                        {
                            items.Add(item);
                            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Item: {item.Id}. {item._partitionKey}");

                        }

                    }

                    RedIdRecord = new List<RedIdRecord>();
                    RedIdRecord = items;

                    if (RedIdRecord == null)
                    {
                        LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Item to search for was found.  CompanyName: {idOut}");
                    }

                }
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Exception, ex.Message);
            }

            return Page();
        }
    }
}
