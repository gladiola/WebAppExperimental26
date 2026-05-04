using Microsoft.AspNetCore.Mvc;
using REDRFID.Models.Settings;

namespace REDRFID.Services
{

    public interface IAzureADSettingsService {

        AzureADSettings GetSettings();

    }
    public class AzureADSettingsService : IAzureADSettingsService
    {
        private readonly AzureADSettings _settings;

        public AzureADSettingsService(AzureADSettings aads) { 
        
            _settings = aads;
        
        }

        
        public AzureADSettings GetSettings()
        {
            return _settings;
        }
        


    }
}
