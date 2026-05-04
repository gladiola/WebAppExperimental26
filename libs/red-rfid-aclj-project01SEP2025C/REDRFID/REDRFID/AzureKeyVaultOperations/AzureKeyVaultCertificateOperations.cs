using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using REDRFID.Models.Main_Objects;
using REDRFID.Services;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace REDRFID.AzureKeyVaultOperations
{
    public interface IAzureKeyVaultCertificateOperations {

        public Task<X509Certificate2> GetCertificateFromKeyVault(string tenantId, string clientId, string keyVaultUrl, string certificateName, string certPasswordName);


        public Task<KeyVaultSecret> GetSecretFromKeyVault(string tenantId, string clientId, string clientSecret, string keyVaultUrl, string secretName);


    }

    public class AzureKeyVaultCertificateOperations : IAzureKeyVaultCertificateOperations
    {
       private readonly ILogger<AzureKeyVaultCertificateOperations> _logger;


        public AzureKeyVaultCertificateOperations(ILogger<AzureKeyVaultCertificateOperations> logger) {

            // Add in logging service
            _logger = logger;

        }

        // Secret Client Class
        // REF:  https://learn.microsoft.com/en-us/dotnet/api/azure.security.keyvault.secrets.secretclient?view=azure-dotnet

        // x509 Certificate 2 class
        //REF:  https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2?view=net-9.0

        public async Task<X509Certificate2> GetCertificateFromKeyVault(string tenantId, string clientId, string keyVaultUrl, string certificateName, string certPasswordName)
        {
            //LoggingHelper.LogDataProcessingStatusServiceWork(_logger, "AzureKeyVaultCertificateOperations.GetCertificateFromKeyVault", string.Empty, DataProcessingStatus.Info, $"beginning calls to get certificates from Azure");

            X509Certificate2 result = null;

            try
            {
                // upon deployment, we threw an exception at DefaultAzureCredential
                // https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/identity/Azure.Identity/TROUBLESHOOTING.md#troubleshoot-defaultazurecredential-authentication-issues
                //  CredentialUnavailableException
                //  We will need to set up some environment variables to make it work better.

                var clientCertificateCredential = new ClientCertificateCredential(tenantId, clientId, certificateName);
                var secretClient = new SecretClient(new Uri(keyVaultUrl), clientCertificateCredential);


                KeyVaultSecret certificateSecret = await secretClient.GetSecretAsync(certificateName);
                string pfxString = certificateSecret.Value;

                if (pfxString.Length > 0)
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, "AzureKeyVaultCertificateOperations.GetCertificateFromKeyVault", string.Empty, DataProcessingStatus.Success, $"Certificate length greater than zero.");
                }

                // Check if the string is Base64-encoded
                byte[] pfxBytes = Convert.FromBase64String(pfxString);
                

                KeyVaultSecret certPassword = await secretClient.GetSecretAsync(certPasswordName);
                string pfxPassphrase = certPassword.Value;
                if (pfxPassphrase.Length > 0)
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, "AzureKeyVaultCertificateOperations.GetCertificateFromKeyVault", string.Empty, DataProcessingStatus.Success, $"Cert password length greater than zero.");
                }

                // Updated to use X509Certificate2.CreateFromPem
                // var certificate = X509Certificate2.CreateFromPem(pemString, string.Empty);
                // throwing an exception but using the correct password.
                var certificate = new X509Certificate2(pfxBytes, pfxPassphrase, X509KeyStorageFlags.MachineKeySet);

                result = certificate;
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"Error loading PFX: {ex.Message}");

               LoggingHelper.LogDataProcessingStatusServiceWork(_logger, "AzureKeyVaultCertificateOperations.GetCertificateFromKeyVault", string.Empty, DataProcessingStatus.Exception, $"Error loading PFX: {ex.Message}");

            }
            catch (Exception ex){

                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, "AzureKeyVaultCertificateOperations.GetCertificateFromKeyVault", string.Empty, DataProcessingStatus.Exception, $"Exception: {ex.Message}");

            }

            return result;

        }





        // REF:  https://www.google.com/search?q=c%23+asp+net+core+pfx+full+chain+to+cert+pkcs12+example&client=ms-android-att-us-rvc3&sca_esv=0f9b108d57af51d0&ei=Eni3aOruNIe1wN4PtsKA6AY&oq=c%23+asp+net+core+pfx+full+chain+to+cert&gs_lp=Egxnd3Mtd2l6LXNlcnAiJmMjIGFzcCBuZXQgY29yZSBwZnggZnVsbCBjaGFpbiB0byBjZXJ0KgIIATIFECEYoAEyBRAhGKABMgUQIRigATIFECEYoAEyBRAhGKABSLgzUPseWPsecAR4AJABAJgBnQGgAYsCqgEDMC4yuAEByAEA-AEBmAIFoAK9AcICChAAGEcY1gQYsAOYAwCIBgGQBgiSBwM0LjGgB-AMsgcDMC4xuAeiAcIHAzItNcgHEw&sclient=gws-wiz-serp


        // REF: https://learn.microsoft.com/en-us/azure/app-service/configure-ssl-certificate-in-code?tabs=windows
        // The certs need additional authorization in Azure or the service will shut down.

        public async Task<KeyVaultSecret> GetSecretFromKeyVault(string tenantId, string clientId, string clientSecret, string keyVaultUrl, string secretName)
        {

            KeyVaultSecret answer = new KeyVaultSecret(keyVaultUrl, secretName);

            try
            {
                var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);  
                var secretClient = new SecretClient(new Uri(keyVaultUrl), clientSecretCredential);


                answer = await secretClient.GetSecretAsync(secretName);

            }
            catch (CryptographicException ex)
            {

                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, "AzureKeyVaultCertificateOperations.GetSecretFromKeyVault", string.Empty, DataProcessingStatus.Exception, $"Cryptographic Exception {ex.Message}");

            }
            catch (Exception ex)
            {

                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, "AzureKeyVaultCertificateOperations.GetCertificateFromKeyVault", string.Empty, DataProcessingStatus.Exception, $"Exception: {ex.Message}");

            }

            return answer;

        }


    }



}
