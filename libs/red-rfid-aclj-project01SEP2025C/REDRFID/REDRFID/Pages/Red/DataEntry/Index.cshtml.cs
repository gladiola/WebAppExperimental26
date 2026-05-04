using Azure.Core;
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
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace REDRFID.Pages.Red.DataEntry
{
    public class IndexModel : PageModel
    {
        private readonly RedCosmosDBContext _context;

        private readonly ILogger<IndexModel> _logger;

        private ICosmosDbSettingsService _cosmosDbSettingsService;

        public IndexModel(RedCosmosDBContext context, ILogger<IndexModel> logger, ICosmosDbSettingsService cosmosDbSettingsService)
        {
            _context = context;
            _logger = logger;
            _cosmosDbSettingsService = cosmosDbSettingsService;
        }

        public IList<RedIdRecord> RedIdRecord { get;set; } = new List<RedIdRecord>();

        public int ListLength { get; set; } = 0;

        public async Task OnGetAsync()
        {
            string caller = "/Red/DataEntry/IndexModel.OnGetAsync()";
            try
            {
                using (_context) {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Using context {_context.ContextId}");

                    Database database = _context.Database.GetCosmosClient().GetDatabase(_cosmosDbSettingsService.GetSettings().DatabaseName);

                    database = await database.ReadAsync();
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Got database:\t{database.Id}");

                    Container container = _context.Database.GetCosmosClient().GetContainer(_cosmosDbSettingsService.GetSettings().DatabaseName, _cosmosDbSettingsService.GetSettings().ContainerName);

                    container = await container.ReadContainerAsync();
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Got container:\t{container.Id}");

                    var query = new QueryDefinition(
                        query: "SELECT * FROM c "
                    );

                    using FeedIterator<RedIdRecord> feed = container.GetItemQueryIterator<RedIdRecord>(queryDefinition: query);

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

                    

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Received: {items.Count()}");

                    RedIdRecord = items;

                    ListLength = RedIdRecord.Count;
                }
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Exception, ex.Message);
            }

            await Task.Delay(1);



        }
    }
}
