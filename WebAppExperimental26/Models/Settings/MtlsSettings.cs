namespace WebAppExperimental26.Models.Settings
{
    /// <summary>
    /// Configuration settings for mutual TLS (mTLS) client certificate authentication
    /// </summary>
    public class MtlsSettings
    {
        /// <summary>
        /// Enable or disable client certificate validation
        /// </summary>
        public bool RequireClientCertificate { get; set; } = true;

        /// <summary>
        /// Allow chained certificates (not self-signed)
        /// </summary>
        public bool AllowCertificateChains { get; set; } = true;

        /// <summary>
        /// Allow self-signed certificates (for development only)
        /// </summary>
        public bool AllowSelfSignedCertificates { get; set; } = false;

        /// <summary>
        /// Perform certificate revocation check
        /// </summary>
        public bool CheckCertificateRevocation { get; set; } = false;

        /// <summary>
        /// Name of the client certificate in Azure Key Vault (optional)
        /// </summary>
        public string? ClientCertificateName { get; set; }

        /// <summary>
        /// Additional validation logic can be configured
        /// </summary>
        public bool ValidateClientCertificateIssuer { get; set; } = true;
    }
}
