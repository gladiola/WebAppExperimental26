using Azure;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos;
using REDRFID.Services;
using REDRFID.Models.Main_Objects;


namespace REDRFID.Models.Storage
{
    public class RedCosmosDBContext : DbContext
    {

        /// <summary>
        /// Constructor for NoteKeeperDBContext class.
        /// </summary>
        /// <param name="options">Options</param>
        public RedCosmosDBContext(DbContextOptions<RedCosmosDBContext> options, ILogger<RedCosmosDBContext> logger, ICosmosDbSettingsService cosmosDbSettingsService) : base(options)
        {
            _logger = logger;
            _cosmosDbSettingsService = cosmosDbSettingsService;
            //Database.EnsureCreated();  cannot be used due to cosmos async requirements.
            //Database.EnsureCreatedAsync();  this command was probably dropping the database
        }

        #region DBContext to DB tables

        /// <summary>
        /// Entity representation of red_id table.
        /// </summary>
        public DbSet<RedIdRecord> RedRecords { get; set; }

        private ILogger<RedCosmosDBContext> _logger;

        private ICosmosDbSettingsService _cosmosDbSettingsService;

        #endregion

        /// <summary>
        /// Set up Entity Framework to map the Note table to Tags with FK NoteId.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            string? container = _cosmosDbSettingsService.GetSettings().ContainerName;
            LoggingHelper.LogDataProcessingStatusServiceWork(_logger, "", DataProcessingStatus.Info, $"RedCosmosDBContext.OnModelCreating() Container: {container}");
            modelBuilder.Entity<RedIdRecord>()
             .ToContainer(container)
             .HasPartitionKey("_partitionKey");
        }

        /// <summary>
        /// Configure enhanced logging
        /// </summary>
        /// <param name="optionsBuilder">The operation builder</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCosmos(
                _cosmosDbSettingsService.GetSettings().AccountEndpoint,
                _cosmosDbSettingsService.GetSettings().AccountKey,
                _cosmosDbSettingsService.GetSettings().DatabaseName
                );

            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }


    }
}
