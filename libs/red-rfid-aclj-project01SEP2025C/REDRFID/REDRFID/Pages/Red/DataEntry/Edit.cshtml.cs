using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using REDRFID.Data;
using REDRFID.Models.Main_Objects;
using REDRFID.Models.Storage;
using REDRFID.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace REDRFID.Pages.Red.DataEntry
{
    public class EditModel : PageModel
    {
        private readonly RedCosmosDBContext _context;

        private readonly ILogger<EditModel> _logger;

        private ICosmosDbSettingsService _cosmosDbSettingsService;

        public string? RequestId { get; set; }

        public EditModel(RedCosmosDBContext context, ILogger<EditModel> logger, ICosmosDbSettingsService cosmosDbSettingsService)
        {
            _context = context;
            _logger = logger;
            _cosmosDbSettingsService = cosmosDbSettingsService;
        }


        [BindProperty]
        public RedIdRecord RedIdRecord { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            string caller = "/Red/DataEntry/EditModel.OnGetAsync()";

            if (id == null)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"id parameter was null");
                return NotFound();
            }


            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Failure, $"User was not properly identified");

                return Forbid(); // Respond with a 403 if user is not authenticated
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

                    Microsoft.Azure.Cosmos.Container container = _context.Database.GetCosmosClient().GetContainer(_cosmosDbSettingsService.GetSettings().DatabaseName, _cosmosDbSettingsService.GetSettings().ContainerName);

                    container = await container.ReadContainerAsync();
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Got container:\t{container.Id}");

                    var query = new QueryDefinition(query: "SELECT * FROM RedRecords c WHERE c.id = @id").WithParameter("@id", idOut);

                    using FeedIterator<RedIdRecord> feed = container.GetItemQueryIterator<RedIdRecord>(queryDefinition: query);

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Feed status after query for SELECT {idOut}:  feed: {feed.ToString()}");

                    while (feed.HasMoreResults)
                    {
                        FeedResponse<RedIdRecord> response = await feed.ReadNextAsync();
                        var redidrecord = response.FirstOrDefault<RedIdRecord>() ?? new RedIdRecord();
                        RedIdRecord = redidrecord;
                        return Page();
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


        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            

            // Log who is calling
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            string caller = "/Red/DataEntry/EditModel.OnPostAsync()";
            LoggingHelper.LogTraceIdentifier(_logger, RequestId);
            string noteRequestId = LoggingHelper.LogRequestReturnId(Request, _logger, caller);
            await LoggingHelper.LogUserActivity(User, _logger, caller, noteRequestId);

            if (!ModelState.IsValid)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Error, "Model state was not valid.");
                return Page();
            }

            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Success, "Model state was valid.");

            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Failure, $"User was not properly identified");

                return Forbid(); // Respond with a 403 if user is not authenticated
            }



            try
            {
                using (_context)
                {

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Using context {_context.ContextId}");

                    Database database = _context.Database.GetCosmosClient().GetDatabase(_cosmosDbSettingsService.GetSettings().DatabaseName);

                    database = await database.ReadAsync();
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Got database:\t{database.Id}");

                    Microsoft.Azure.Cosmos.Container container = _context.Database.GetCosmosClient().GetContainer(_cosmosDbSettingsService.GetSettings().DatabaseName, _cosmosDbSettingsService.GetSettings().ContainerName);

                    container = await container.ReadContainerAsync();
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Got container:\t{container.Id}");

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Intend to update item Id: \t{id}");

                    var itemToUpdate = await _context.RedRecords
            .FirstOrDefaultAsync(e => e.Id == id);

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Before changes, Record: {itemToUpdate.ToString()}" +
                        $" {itemToUpdate.FacilityCode}" +
                        $" {itemToUpdate.CardNumber}" +
                        $" {itemToUpdate.ParityBits}" +
                        $" {itemToUpdate.LocationAddress}" +
                        $" {itemToUpdate.LocationCity}" +
                        $" {itemToUpdate.LocationState}" +
                        $" {itemToUpdate.LocationZip}" +
                        $" {itemToUpdate.LocationLat}" +
                        $" {itemToUpdate.LocationLong}" +
                        $" {itemToUpdate.RfidRisk}" +
                        $" {itemToUpdate.CompanyName}" +
                        $" {itemToUpdate.RFIDContent.ToString()}");

                    if (itemToUpdate != null)
                    {
                        // substitute user-provided data
                        itemToUpdate.FacilityCode = RedIdRecord.FacilityCode;
                        itemToUpdate.CardNumber = RedIdRecord.CardNumber;
                        itemToUpdate.ParityBits = RedIdRecord.ParityBits;
                        itemToUpdate.LocationAddress = RedIdRecord.LocationAddress;
                        itemToUpdate.LocationCity = RedIdRecord.LocationCity;
                        itemToUpdate.LocationState = RedIdRecord.LocationState;
                        itemToUpdate.LocationZip = RedIdRecord.LocationZip;
                        itemToUpdate.LocationLat = RedIdRecord.LocationLat;
                        itemToUpdate.LocationLong = RedIdRecord.LocationLong;
                        itemToUpdate.RfidRisk = RedIdRecord.RfidRisk;
                        itemToUpdate.CompanyName = RedIdRecord.CompanyName;
                        itemToUpdate.RFIDContent = RedIdRecord.RFIDContent;
                        // note who made the change
                        itemToUpdate.UserID = HttpContext.TraceIdentifier.ToString();

                        LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, "Async saving changes.");

                        var changedItems = await _context.SaveChangesAsync();

                        LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"after async save changes reported {changedItems.ToString()} state entries written to database");

                        LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"After changes values should be, Record: {itemToUpdate.ToString()}" +
                            $" {itemToUpdate.FacilityCode}" +
                            $" {itemToUpdate.CardNumber}" +
                            $" {itemToUpdate.ParityBits}" +
                            $" {itemToUpdate.LocationAddress}" +
                            $" {itemToUpdate.LocationCity}" +
                            $" {itemToUpdate.LocationState}" +
                            $" {itemToUpdate.LocationZip}" +
                            $" {itemToUpdate.LocationLat}" +
                            $" {itemToUpdate.LocationLong}" +
                            $" {itemToUpdate.RfidRisk}" +
                            $" {itemToUpdate.CompanyName}" +
                            $" {itemToUpdate.RFIDContent.ToString()}");

                    }

                    if (RedIdRecord == null)
                    {
                        LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Item to edit was not found.  id: {id}");
                    }

                }

            }
            catch (DbUpdateConcurrencyException ex)
            {
                bool exists = await RedIdRecordExists(RedIdRecord.Id);
                if (exists)
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Exception, "Record exists.");

                    return NotFound();
                }
                else
                {

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Exception, ex.Message);
                    //throw;
                }
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Exception, ex.Message);
            }


            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, "Redirecting.");


            return RedirectToPage("./Index");
        }

        private async Task<bool> RedIdRecordExists(int id)
        {
            bool answer = false;

            string caller = "/Red/DataEntry/EditModel.RedIdRecordExists()";

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

                    Microsoft.Azure.Cosmos.Container container = _context.Database.GetCosmosClient().GetContainer(_cosmosDbSettingsService.GetSettings().DatabaseName, _cosmosDbSettingsService.GetSettings().ContainerName);

                    container = await container.ReadContainerAsync();
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Got container:\t{container.Id}");

                    var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.id = @id").WithParameter("@id", idOut);

                    using FeedIterator<int> feed = container.GetItemQueryIterator<int>(queryDefinition: query);

                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Feed status after query for SELECT {idOut}:  feed: {feed.ToString()}");

                    if (feed.HasMoreResults)
                    {
                        var response = await feed.ReadNextAsync();
                        if (response.Count > 0)
                        {
                            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Found {response.Count} while looking for {idOut}");
                            // We found some, if we got at least one it's more than zero.
                            return response.First() > 0;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Exception, ex.Message);
            }


            return answer;
        }
    }
}
