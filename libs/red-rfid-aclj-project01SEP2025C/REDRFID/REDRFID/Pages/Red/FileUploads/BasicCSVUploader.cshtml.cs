using Azure.Core;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Cosmos;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using REDRFID.Models.Main_Objects;
using REDRFID.Models.Storage;
using REDRFID.Services;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;

namespace REDRFID.Pages.Red.FileUploads
{

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    //[Authorize(Policy = "DefaultPolicy")]
    public class BasicCSVUploaderModel : PageModel
    {

        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly RedCosmosDBContext _context;

        private readonly ILogger<BasicCSVUploaderModel> _logger;

        private ICosmosDbSettingsService _cosmosDbSettingsService;

        public BasicCSVUploaderModel(RedCosmosDBContext context, ILogger<BasicCSVUploaderModel> logger, ICosmosDbSettingsService cosmosDbSettingsService) {

            _context = context;
            _logger = logger;
            _cosmosDbSettingsService = cosmosDbSettingsService;

        }


        public IList<RedIdRecord> RedIdRecord { get; set; } = new List<RedIdRecord>();

        public int ListLength { get; set; } = 0;

        [BindProperty]
        public IFormFile CsvDataIn { get; set; }


        public async void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            // Log who is calling
            string caller = "BasicCSVUploaderModel.OnGet()";
            LoggingHelper.LogTraceIdentifier(_logger, RequestId);
            string noteRequestId = LoggingHelper.LogRequestReturnId(Request, _logger, caller);
            await LoggingHelper.LogUserActivity(User, _logger, caller, noteRequestId);


        }



        public async Task<IActionResult> OnPost() {

            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            // Log who is calling
            string caller = "BasicCSVUploaderModel.OnPost()";
            LoggingHelper.LogTraceIdentifier(_logger, RequestId);
            string noteRequestId = LoggingHelper.LogRequestReturnId(Request, _logger, caller);
            await LoggingHelper.LogUserActivity(User, _logger, caller, noteRequestId);


            // Ensure there is some data to process
            if (CsvDataIn == null || CsvDataIn.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "No file uploaded.");
                return Page();
            }


            using (var reader = new StreamReader(CsvDataIn.OpenReadStream()))
            using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.Configuration.BadDataFound = context =>
                {
                    // Handle the bad data (e.g., log it)
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Bad data found in line: {context.RawRecord}");
                };

                var records = csv.GetRecords<RedIdRecordCSVEntry>().ToList();
                // Process records as needed

                foreach (var record in records)
                {
                    // Log each record uploaded.
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Info, $"Record: {record.ToString()}" +
                        $" {record.FacilityCode}" +
                        $" {record.CardNumber}" +
                        $" {record.ParityBits}" +
                        $" {record.LocationAddress}" +
                        $" {record.LocationCity}" +
                        $" {record.LocationState}" +
                        $" {record.LocationZip}" +
                        $" {record.LocationLat}" +
                        $" {record.LocationLong}" +
                        $" {record.RfidRisk}" +
                        $" {record.CompanyName}" +
                        $" {record.RFIDContent.ToString()}");

                    var fullRecord = new RedIdRecord();
                    fullRecord.Id = Random.Shared.Next(10, int.MaxValue);
                    fullRecord.UploadedUser = HttpContext.TraceIdentifier.ToString();
                    fullRecord.UserID = HttpContext.TraceIdentifier.ToString();
                    fullRecord.IdString = Guid.NewGuid().ToString();
                    fullRecord._partitionKey = "85F05E2D1492C89F3EC1052A";

                    fullRecord.FacilityCode = record.FacilityCode;
                    fullRecord.CardNumber = record.CardNumber;
                    fullRecord.ParityBits = record.ParityBits;
                    fullRecord.LocationAddress = record.LocationAddress;
                    fullRecord.LocationCity = record.LocationCity;
                    fullRecord.LocationState = record.LocationState;
                    fullRecord.LocationZip = record.LocationZip;
                    fullRecord.LocationLat = record.LocationLat;
                    fullRecord.LocationLong = record.LocationLong;
                    fullRecord.RfidRisk = record.RfidRisk;
                    fullRecord.CompanyName = record.CompanyName;
                    fullRecord.RFIDContent = record.RFIDContent;

                    RedIdRecord.Add( fullRecord );

                }

                // Upload these records to CosmosDB
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

                        foreach (var item in RedIdRecord) { 
                            await _context.AddAsync(item);
                        }

                        await _context.SaveChangesAsync();

                    }
                }
                catch (Exception ex)
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, DataProcessingStatus.Exception, ex.Message);
                }

            }

            // Later redirect in a way that makes sense and shows the upload
            return Page();


        }



    }
}
