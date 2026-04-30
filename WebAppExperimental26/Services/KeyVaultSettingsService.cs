using WebAppExperimental26.Models.Settings;

namespace WebAppExperimental26.Services
{

    public interface IKeyVaultSettingsService { 
        
        KeyVaultSettings GetSettings();

    }
    public class KeyVaultSettingsService : IKeyVaultSettingsService
    {
        private readonly KeyVaultSettings _settings;

        public KeyVaultSettingsService(KeyVaultSettings kvs) { 
        
            _settings = kvs;
        
        }

        public KeyVaultSettings GetSettings()
        {
            return _settings;
        }
    }
}
