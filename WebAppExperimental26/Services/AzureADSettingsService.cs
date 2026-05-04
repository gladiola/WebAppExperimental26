using Microsoft.AspNetCore.Mvc;
using WebAppExperimental26.Models.Settings;

namespace WebAppExperimental26.Services
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
