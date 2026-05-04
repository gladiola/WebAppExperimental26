using Microsoft.Extensions.Hosting;
using System.Text;
using WebAppExperimental26.Models.Main_Objects;
using WebAppExperimental26.Models.Settings;

namespace WebAppExperimental26.Services
{
    public class ContentSecurityPolicyBuilder
    {
        /// <summary>
        /// Build CSP with nonce and script hashes
        /// </summary>
        public string BuildCSPWithNonceAndHashes(string nonce, string? hashFilePath, CSPScriptHashSettings? csphs)
        {
            StringBuilder cspBuilder = new StringBuilder();
            
            cspBuilder.Append("default-src 'none'; ");
            cspBuilder.Append("script-src 'self' https://www.physicalsecurityatlas.com 'nonce-");
            cspBuilder.Append(nonce);
            cspBuilder.Append("'");

            // Add script hashes if file exists
            if (!string.IsNullOrEmpty(hashFilePath) && File.Exists(hashFilePath))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(hashFilePath))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                cspBuilder.Append(" '");
                                cspBuilder.Append(line.Trim());
                                cspBuilder.Append("'");
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    // Silently continue if file can't be read
                }
            }

            // Add manual hash if provided
            if (csphs != null && !string.IsNullOrEmpty(csphs.ManuallyCalculatedInlineHash1))
            {
                cspBuilder.Append(" '");
                cspBuilder.Append(csphs.ManuallyCalculatedInlineHash1);
                cspBuilder.Append("'");
            }

            cspBuilder.Append("; connect-src 'self'; img-src 'self' data:; ");
            cspBuilder.Append("style-src 'self' https://experience.arcgis.com https://harvard-cga.maps.arcgis.com; ");
            cspBuilder.Append("frame-src https://experience.arcgis.com https://harvard-cga.maps.arcgis.com; ");
            cspBuilder.Append("frame-ancestors 'self'; form-action 'self';");

            return cspBuilder.ToString();
        }

        /// <summary>
        /// Given a filename that holds post-build computed hashes, add them in to the CSP
        /// </summary>
        public string BuildCSPWithHashesForScripts(ILogger logger, string filePath, CSPScriptHashSettings csphs) { 
        
            StringBuilder CSPScriptHashes = new StringBuilder();
           // string preamble = "default-src 'none'; script-src 'self' ";
            string preamble = "script-src ";
            string conclusion = " ; connect-src 'self'; img-src 'self'; style-src 'self'; frame-ancestors 'self'; form-action 'self';";

            CSPScriptHashes.Append(preamble);

            try
            {
                string fullFilePath = Path.Combine(filePath);

                // Read the file line by line
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        CSPScriptHashes.Append("'");
                        CSPScriptHashes.Append(line);
                        CSPScriptHashes.Append("' ");
                    }
                }
            }
            catch (IOException ex)
            {
                LoggingHelper.LogDataProcessingStatusServiceWork(logger, "", DataProcessingStatus.Exception, ex.Message);

            }

            // pull one more manual value from config
            CSPScriptHashes.Append(csphs.ManuallyCalculatedInlineHash1);

            CSPScriptHashes.Append(conclusion);
            return CSPScriptHashes.ToString();


        }

    }
}
