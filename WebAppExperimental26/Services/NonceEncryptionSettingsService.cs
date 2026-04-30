using REDRFID.Models.Settings;

namespace REDRFID.Services
{

    public interface INonceEncryptionSettingsService {

        NonceEncryptionSettings GetSettings();

    }
    public class NonceEncryptionSettingsService : INonceEncryptionSettingsService
    {
        private readonly NonceEncryptionSettings _settings;

        public NonceEncryptionSettingsService(NonceEncryptionSettings nes) { 
        
            _settings = nes;
        
        }

        public NonceEncryptionSettings GetSettings()
        {
            return _settings;
        }
    }
}
