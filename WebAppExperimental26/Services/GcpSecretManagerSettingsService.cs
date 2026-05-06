using WebAppExperimental26.Models.Settings;

namespace WebAppExperimental26.Services
{
    public interface IGcpSecretManagerSettingsService
    {
        GcpSecretManagerSettings GetSettings();
    }

    public class GcpSecretManagerSettingsService : IGcpSecretManagerSettingsService
    {
        private readonly GcpSecretManagerSettings _settings;

        public GcpSecretManagerSettingsService(GcpSecretManagerSettings settings)
        {
            _settings = settings;
        }

        public GcpSecretManagerSettings GetSettings()
        {
            return _settings;
        }
    }
}
