namespace WebAppExperimental26.Models.Settings
{
    /// <summary>
    /// Class to decsribe configuration values for encrypting a random number as nonce.
    /// </summary>
    public class NonceEncryptionSettings
    {

        public required string KeyVaultURL { get; set; }
        public required string IVSecret { get; set; }
        public required string NonceKeySecret { get; set; }

    }
}
