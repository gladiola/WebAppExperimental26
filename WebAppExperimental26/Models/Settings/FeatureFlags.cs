namespace WebAppExperimental26.Models.Settings
{
    public class FeatureFlags
    {
        public bool EnableSession { get; set; } = true;
        public bool EnableLocalization { get; set; } = true;
        public bool EnableAzureAd { get; set; } = true;
        public bool EnableAuthorization { get; set; } = true;
        public bool EnableKeyVault { get; set; } = false;
        public bool EnableNonceServices { get; set; } = true;
        public bool EnableBlobStorage { get; set; } = false;
        public bool EnableCosmosDb { get; set; } = false;
        public bool EnableSecurityHeaders { get; set; } = true;
        public bool EnableCSP { get; set; } = true;
        public bool EnableMtls { get; set; } = false;
        public bool EnableOcspValidation { get; set; } = false;
    }
}