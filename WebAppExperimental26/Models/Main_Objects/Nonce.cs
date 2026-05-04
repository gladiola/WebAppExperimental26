using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using WebAppExperimental26.Services;
using System.Security.Cryptography;

namespace WebAppExperimental26.Models.Main_Objects
{
    public interface INonce
    {
        string GetNonceAsString();
    }

    public class Nonce : INonce
    {
        private readonly string _nonce;
        private readonly ILogger<Nonce> _logger;

        /// <summary>
        /// Utility class to encrypt a random number with a key stored in KeyVault.
        /// </summary>
        public Nonce(ILogger<Nonce> logger, KeyVaultSecret iv, KeyVaultSecret key)
        {
            _logger = logger;
            _nonce = GenerateEncryptedNonce(iv, key);
        }

        /// <summary>
        /// Use an IV and a Key from KeyVault to encrypt a random number.
        /// </summary>
        /// <param name="iv">From KeyVault</param>
        /// <param name="key">From KeyVault</param>
        /// <returns>A hex string of an encrypted random number</returns>
        public string GenerateEncryptedNonce(KeyVaultSecret iv, KeyVaultSecret key)
        {
            string answer = string.Empty;
            string caller = "Nonce.GenerateEncryptedNonce()";

            byte[] ivBytes = Convert.FromHexString(iv.Value);
            byte[] keyBytes = Convert.FromHexString(key.Value);
            byte[] randomNumber = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            string randomNumberAsString = BitConverter.ToString(randomNumber).Replace("-", "");

            if (!string.IsNullOrEmpty(randomNumberAsString)
                && randomNumber != null
                && ivBytes != null
                && keyBytes != null)
            {
                try
                {
                    using AesGcm aesGcm = new AesGcm(keyBytes, 16);
                    byte[] ciphertext = new byte[randomNumber.Length];
                    byte[] tag = new byte[16];
                    aesGcm.Encrypt(ivBytes, randomNumber, ciphertext, tag);
                    byte[] result = new byte[ciphertext.Length + tag.Length];
                    Array.Copy(ciphertext, 0, result, 0, ciphertext.Length);
                    Array.Copy(tag, 0, result, ciphertext.Length, tag.Length);
                    answer = Convert.ToBase64String(result);
                }
                catch (ArgumentNullException ex)
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, "", DataProcessingStatus.Exception, $"Nonce generation exception {ex.Message}");
                }
                catch (ArgumentException ex)
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, "", DataProcessingStatus.Exception, $"Nonce generation exception {ex.Message}");
                }
                catch (Exception ex)
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, "", DataProcessingStatus.Exception, $"Nonce generation exception {ex.Message}");
                }
            }
            else
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, caller, "", DataProcessingStatus.Failure, $"Nonce missing starting components.");
            }

            return answer;
        }

        /// <summary>
        /// Accessor for nonce value
        /// </summary>
        /// <returns>String with nonce in characters.</returns>
        public string GetNonceAsString()
        {
            return _nonce;
        }
    }
}
