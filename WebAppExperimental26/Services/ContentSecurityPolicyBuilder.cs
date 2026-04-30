using Microsoft.Extensions.Hosting;
using System.Text;
using WebAppExperimental26.Models.Main_Objects;
using WebAppExperimental26.Models.Settings;

namespace REDRFID.Services
{
    public class ContentSecurityPolicyBuilder
    {



        /// <summary>
        /// Given a filename that holds post-build computed hashes, add them in to the CSP
        /// </summary>
        /// <returns></returns>
        public string BuildCSPWithHashesForScripts(ILogger logger, string filePath, CSPScriptHashSettings csphs) { 
        
            StringBuilder CSPScriptHashes = new StringBuilder();
           // string preamble = "default-src 'none'; script-src 'self' ";
            string preamble = "script-src ";
            string conclusion = " ; connect-src 'self'; img-src 'self'; style-src 'self'; frame-ancestors 'self'; form-action 'self';";

            CSPScriptHashes.Append(preamble);

            try
            {
                string fullFilePath = System.IO.Path.Combine(filePath);

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
