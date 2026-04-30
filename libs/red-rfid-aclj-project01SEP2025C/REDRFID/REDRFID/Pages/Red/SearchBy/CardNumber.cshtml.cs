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
    public class CardNumberModel : PageModel
    {
        private readonly RedCosmosDBContext _context;

        private readonly ILogger<CardNumberModel> _logger;

        private ICosmosDbSettingsService _cosmosDbSettingsService;

        public CardNumberModel(RedCosmosDBContext context, ILogger<CardNumberModel> logger, ICosmosDbSettingsService cosmosDbSettingsService)
        {
            _context = context;
            _logger = logger;
            _cosmosDbSettingsService = cosmosDbSettingsService;
        }

        [BindProperty]
        public IList<RedIdRecord> RedIdRecord { get;set; } = new List<RedIdRecord>();

        [BindProperty]
        public string CardNumber { get; set; }

        public int ListLength { get; set; } = 0;


        public async Task<IActionResult> OnGetAsync()
        {
            string caller = "/Red/SearchBy/CardNumberModel.OnGetAsync()";
            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"OnGetAsync was called.  No data processing needed.");

            await Task.Delay(1);

            return Page();


        }
        


        public async Task<IActionResult> OnPostAsync()
        {
            string caller = "/Red/DataEntry/CardNumberModel.OnPostAsync()";

            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Function OnPostAsync() begins in CardNumber");

            if (!ModelState.IsValid) {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"ModelState was not valid");
                return Page();
            }

            if (String.IsNullOrEmpty(CardNumber))
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"search parameter was null");
                return NotFound();
            }

            // Convert the integer to a string to go better with the query below.
            string idOut = Convert.ToString(CardNumber);

            try
            {
                using (_context)
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Using context {_context.ContextId}");

                    Database database = _context.Database.GetCosmosClient().GetDatabase(_cosmosDbSettingsService.GetSettings().DatabaseName);

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Will use database Id:  {database.Id}");

                    database = await database.ReadAsync();
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Got database:\t{database.Id}");

                    Container container = _context.Database.GetCosmosClient().GetContainer(_cosmosDbSettingsService.GetSettings().DatabaseName, _cosmosDbSettingsService.GetSettings().ContainerName);

                    container = await container.ReadContainerAsync();
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Got container:\t{container.Id}");

                    var query = new QueryDefinition(query: "SELECT * FROM RedRecords c WHERE c.CardNumber = @CardNumber").WithParameter("@CardNumber", idOut);

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
                        LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Item to search for was found.  CardNumber: {idOut}");
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
