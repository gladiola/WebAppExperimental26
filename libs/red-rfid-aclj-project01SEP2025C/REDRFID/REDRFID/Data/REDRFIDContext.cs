using Microsoft.EntityFrameworkCore;
using REDRFID.Models.Storage;
using REDRFID.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REDRFID.Data
{
    public class REDRFIDContext : DbContext
    {

        private ILogger<RedCosmosDBContext> _logger;

        private ICosmosDbSettingsService _cosmosDbSettingsService;


        public REDRFIDContext (DbContextOptions<REDRFIDContext> options, ILogger<RedCosmosDBContext> logger, ICosmosDbSettingsService cosmosDbSettingsService)
            : base(options)
        {
            _logger = logger;
            _cosmosDbSettingsService = cosmosDbSettingsService;

        }

        public DbSet<REDRFID.Models.Storage.RedIdRecord> RedIdRecord { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the container name here
            // Notice how this partition key does not align with our expectations.
            modelBuilder.Entity<RedIdRecord>()
                .ToContainer("RedRecordsB")
                .HasPartitionKey(r => r.Id);
        }

        /// <summary>
        /// Configure enhanced logging
        /// </summary>
        /// <param name="optionsBuilder">The operation builder</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

    }
}
