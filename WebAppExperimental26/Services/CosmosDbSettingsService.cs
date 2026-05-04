using WebAppExperimental26.Models.Settings;

namespace WebAppExperimental26.Services
{

    public interface ICosmosDbSettingsService {

        CosmosDbSettings GetSettings();

    }
    public class CosmosDbSettingsService : ICosmosDbSettingsService
    {
        private readonly CosmosDbSettings _settings;

        public CosmosDbSettingsService(CosmosDbSettings cdbs) { 
        
            _settings = cdbs;
        
        }

        public CosmosDbSettings GetSettings()
        {
            return _settings;
        }
    }
}
