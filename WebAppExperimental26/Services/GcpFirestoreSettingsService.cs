using WebAppExperimental26.Models.Settings;

namespace WebAppExperimental26.Services
{
    public interface IGcpFirestoreSettingsService
    {
        GcpFirestoreSettings GetSettings();
    }

    public class GcpFirestoreSettingsService : IGcpFirestoreSettingsService
    {
        private readonly GcpFirestoreSettings _settings;

        public GcpFirestoreSettingsService(GcpFirestoreSettings settings)
        {
            _settings = settings;
        }

        public GcpFirestoreSettings GetSettings()
        {
            return _settings;
        }
    }
}
