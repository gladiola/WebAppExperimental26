using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using REDRFID.Models.Settings;
using REDRFID.Models.Storage;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;

namespace REDRFID.Services
{
    public interface IDataServiceInitialCalls { 
    
        public Task GetRedRecordsAsync(Func<string, Task> writeOutputAync);

        public Task GetRedRecordById(Func<string, Task> writeOutputAync, int id);

    }


    public class DataServiceInitialCalls : IDataServiceInitialCalls
    {
        private readonly CosmosDbService _dbService;

        private readonly CosmosDbSettingsService _cosmosSettingsService;

        private readonly ILogger<DataServiceInitialCalls> _logger;


        public DataServiceInitialCalls(CosmosDbService cosmosDbService, CosmosDbSettingsService cosmosSettings, ILogger<DataServiceInitialCalls> logger)  { 
            
            _dbService = cosmosDbService;
            _cosmosSettingsService = cosmosSettings;
            _logger = logger;
        
        }

        public async Task GetRedRecordsAsync(Func<string, Task> writeOutputAync)
        {
            Database database = await _dbService.GetDatabaseAsync();
            Microsoft.Azure.Cosmos.Container container = database.GetContainer(_cosmosSettingsService.GetSettings().ContainerName);

            container = await container.ReadContainerAsync();
            await writeOutputAync($"Get container:\t{container.Id}");

            var query = new QueryDefinition(
                query: "SELECT * FROM RedRecords c WHERE 1=1"
            );

            using FeedIterator<RedIdRecord> feed = container.GetItemQueryIterator<RedIdRecord>(
                queryDefinition: query
            );

            await writeOutputAync($"Ran query:\t{query.QueryText}");

            List<RedIdRecord> items = new();
            double requestCharge = 0d;
            while (feed.HasMoreResults)
            {
                FeedResponse<RedIdRecord> response = await feed.ReadNextAsync();
                foreach (RedIdRecord item in response)
                {
                    items.Add(item);
                }
                requestCharge += response.RequestCharge;
            }

            foreach (var item in items)
            {
                await writeOutputAync($"Found item:\t{item.Id}\t[{item.FacilityCode}]");
            }
            await writeOutputAync($"Request charge:\t{requestCharge:0.00}");
        }

        public async Task GetRedRecordById(Func<string, Task> writeOutputAync, int id)
        {
            Database database = await _dbService.GetDatabaseAsync();
            Microsoft.Azure.Cosmos.Container container = database.GetContainer(_cosmosSettingsService.GetSettings().ContainerName);

            container = await container.ReadContainerAsync();
            await writeOutputAync($"Get container:\t{container.Id}");

            var query = new QueryDefinition(
                query: "SELECT * FROM RedRecords c WHERE c.id = @Id"
            )
                .WithParameter("@Id", id);


            using FeedIterator<RedIdRecord> feed = container.GetItemQueryIterator<RedIdRecord>(
                queryDefinition: query
            );

            await writeOutputAync($"Ran query:\t{query.QueryText}");

            List<RedIdRecord> items = new();
            double requestCharge = 0d;
            while (feed.HasMoreResults)
            {
                FeedResponse<RedIdRecord> response = await feed.ReadNextAsync();
                foreach (RedIdRecord item in response)
                {
                    items.Add(item);
                }
                requestCharge += response.RequestCharge;
            }

            foreach (var item in items)
            {

                foreach (PropertyInfo property in item.GetType().GetProperties())
                {
                    await writeOutputAync($"Found:\t{property.Name}\t{property.GetValue(item)??string.Empty}");
                }

                    
            }
            await writeOutputAync($"Request charge:\t{requestCharge:0.00}");

        }
    }
}
