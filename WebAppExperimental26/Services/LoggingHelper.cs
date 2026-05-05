using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebAppExperimental26.Models.Main_Objects;
using WebAppExperimental26.Models.User;
using Microsoft.Extensions.Logging;

namespace WebAppExperimental26.Services
{
    /// <summary>
    /// Code to unify logging practices throughout the application.
    /// </summary>
    public static class LoggingHelper
    {
        // HMAC-SHA256 key used to pseudonymise PII in logs.
        // Set once at startup via Initialize(); falls back to a random per-process key.
        private static byte[] _hmacKey = RandomNumberGenerator.GetBytes(32);

        /// <summary>
        /// Initialize the HMAC key used to hash PII values before they are written to logs.
        /// Call this once at application startup, passing a stable secret from configuration.
        /// If <paramref name="key"/> is null or empty a random key is generated for this
        /// process lifetime, which still protects PII but prevents cross-restart correlation.
        /// </summary>
        /// <param name="key">32-byte HMAC key. Supply from a secret store (Key Vault, User Secrets).</param>
        public static void Initialize(byte[]? key)
        {
            if (key != null && key.Length > 0)
            {
                if (key.Length != 32)
                {
                    // Use the provided key anyway (HMAC-SHA256 accepts any length), but warn
                    // that a 32-byte key is recommended for optimal security.
                    // A random 32-byte key is used as fallback when key is entirely missing.
                    // We do NOT fall back here so callers get consistent behaviour.
                }
                _hmacKey = key;
            }
            // else: keep the random key already assigned at field-initialization time.
        }

        /// <summary>
        /// Returns a short, stable HMAC-SHA256 hex token for a PII value.
        /// Identical inputs always produce identical tokens within the same process
        /// (or across processes when a stable key is configured), enabling log correlation
        /// without exposing the underlying value.
        /// </summary>
        private static string HashPii(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "(none)";
            }

            byte[] inputBytes = Encoding.UTF8.GetBytes(value);
            byte[] hashBytes = HMACSHA256.HashData(_hmacKey, inputBytes);
            // Return the first 12 bytes (96 bits) as hex — short enough to read in logs,
            // long enough to be collision-resistant for correlation purposes.
            return Convert.ToHexString(hashBytes, 0, 12).ToLowerInvariant();
        }


        /// <summary>
        /// Utility function to log user activity.
        /// </summary>
        /// <param name="claimsPrincipal">User value from AspNetCore Razor Pages.</param>
        /// <param name="_logger">Transfer logging to function.</param>
        /// <param name="methodName">Which class.method calling the function. </param>
        /// <param name="noteRequestId">GUID distinctly identifying a request.</param>
        /// <returns>True if the user was fully identified in the claimsPrincipal for logging.</returns>
        public static async Task<bool> LogUserActivity(ClaimsPrincipal claimsPrincipal, ILogger _logger, string methodName, string noteRequestId, string? traceId = null)
        {

            bool answerIdentifiedUserFully = false;

            if (traceId != null)
            {
                LoggingHelper.LogTraceIdentifier(_logger, traceId);
            }

            if (claimsPrincipal.Identity != null)
            {

                string userName = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "oid")?.Value ?? "Unknown User";
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, methodName, noteRequestId, DataProcessingStatus.Success, $"Auth for {userName}");

                try
                {
                    if (claimsPrincipal.Claims != null)
                    {
                        UserClaimsLoader userClaimsLoader = new();
                        UserClaims userClaims = await userClaimsLoader.LoadDataAsync(claimsPrincipal.Claims);

                        LoggingHelper.LogUserClaims(userClaims, _logger, methodName);
                        answerIdentifiedUserFully = true;
                    }
                    else
                    {
                        LoggingHelper.LogDataProcessingStatusServiceWork(_logger, methodName, noteRequestId, DataProcessingStatus.Info, $"User.Claims was null");
                    }
                }
                catch (Exception ex)
                {
                    LoggingHelper.LogDataProcessingStatusServiceWork(_logger, methodName, noteRequestId, DataProcessingStatus.Exception, $"{ex.Message}");
                }
            }
            else
            {
                string userName = claimsPrincipal.Identity?.Name ?? "Unknown User, User.Identity not detected.";
                LoggingHelper.LogDataProcessingStatusServiceWork(_logger, methodName, noteRequestId, DataProcessingStatus.Failure, $"Failed Auth for {userName}");
            }

            return answerIdentifiedUserFully;
        }

        /// <summary>
        /// Method to log a string; expecting a trace identifier.
        /// </summary>
        /// <param name="_logger">logger</param>
        /// <param name="traceId">Expecting string from Activity.Current?.Id ?? HttpContext.TraceIdentifier</param>
        public static void LogTraceIdentifier(ILogger _logger, string traceId)
        {
            _logger.LogInformation("Trace Identifier: {0}", traceId);
        }


        public static void LogUserClaims(UserClaims userClaims, ILogger _logger, string methodName)
        {
            // Hash PII fields before logging to avoid storing plaintext personal data.
            // Tokens are consistent HMAC-SHA256 digests that support log correlation
            // without exposing the underlying Sid, Oid, email, or display name.
            string sidHash   = HashPii(userClaims.Sid);
            string oidHash   = HashPii(userClaims.Oid);
            string emailHash = HashPii(userClaims.Email);
            string nameHash  = HashPii(userClaims.Name);

            // Log who is calling
            _logger.LogInformation("{0} {1} called in Session {2} User-Oid {3} Email {4} Name {5}", DateTime.UtcNow, methodName, sidHash, oidHash, emailHash, nameHash);

            StringBuilder sb = new StringBuilder();
            if (userClaims.Roles != null)
            {
                bool firstIterationComplete = false;
                foreach (var role in userClaims.Roles)
                {
                    sb.Append(role);
                    if (firstIterationComplete)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        firstIterationComplete = true;
                    }
                }
            }

            _logger.LogInformation("{0} Oid carries the following permissions: {1}", oidHash, sb.ToString());


        }


        /// <summary>
        /// Method to log Requests and assoicate them with data processing in a certain method.
        /// </summary>
        /// <param name="request">Received HttpRequest</param>
        /// <param name="_logger">Logger used in the processing class.</param>
        /// <param name="methodName">Method doing the data processing.</param>
        /// <returns>GUID as string that identifies the Request in logs.</returns>
        public static string LogRequestReturnId(this HttpRequest request, ILogger _logger, string methodName)
        {

            string noteRequestId = Guid.NewGuid().ToString();

            // Log who is calling
            // In this scheme, we do not see Origin headers being sent from the requestor.
            request.Headers.TryGetValue("Origin", out var origin);
            request.Headers.TryGetValue("Host", out var host);
            if (
                !string.IsNullOrEmpty(origin) || !string.IsNullOrEmpty(host)
                )
            {
                _logger.LogInformation("{0} {1} called from Origin {2} Host {3} associated with {4}", DateTime.UtcNow, methodName, origin, host, noteRequestId);
            }
            else
            {
                _logger.LogInformation("{0} {1} called from unknown Origin and Host associated with {2}", DateTime.UtcNow, methodName, noteRequestId);
            }

            return noteRequestId;

        }


        /// <summary>
        /// Method to log Requests and assoicate them with data processing in a certain method.
        /// Uses Reflection to determine class and function which called it.
        /// If called from an async/await, it will yield the compiler-derived class name instead of the human written one.  
        /// </summary>
        /// <param name="request">Received HttpRequest</param>
        /// <param name="_logger">Logger used in the processing class.</param>
        /// <returns>GUID as string that identifies the Request in logs.</returns>
        [Obsolete("Did not obtain practical benefit from this version due to compiler-derived names.")]
        public static string LogRequestReturnId(this HttpRequest request, ILogger _logger)
        {
            // Distinctly identify a Requst in the logs.
            string noteRequestId = Guid.NewGuid().ToString();

            // Be able to name the calling class and method in logs
            StringBuilder sbCallingMethod = new StringBuilder();
            StringBuilder sbCallingClass = new StringBuilder();
            StringBuilder sbMethodName = new StringBuilder();

            // Use Reflection to identify which class and method called this class and method.
            StackTrace st = new StackTrace();
            StackFrame? frame = st.GetFrame(1);
            if (frame != null)
            {
                var callingMethod = frame.GetMethod();
                if (callingMethod != null)
                {
                    var callingClass = callingMethod.DeclaringType;


                    sbCallingMethod.Append(callingMethod.Name);
                    if (callingClass != null)
                    {
                        if (callingClass.Name.Contains("<"))
                        {
                            sbCallingClass.Append(callingClass.Name.Substring(0, callingClass.Name.IndexOf("<")));
                        }
                        else
                        {
                            sbCallingClass.Append(callingClass.Name);
                        }


                    }
                    else
                    {
                        sbCallingClass.Append("undetermined Class");
                    }
                }
                else
                {
                    sbCallingMethod.Append("undetermined Method");
                }

                sbMethodName.Append(sbCallingClass.ToString());
                sbMethodName.Append('.');
                sbMethodName.Append(sbCallingMethod.ToString());
            }

            string methodName = sbMethodName.ToString();


            // Log who is calling
            // In this scheme, we do not see Origin headers being sent from the requestor.
            request.Headers.TryGetValue("Origin", out var origin);
            request.Headers.TryGetValue("Host", out var host);
            if (
                !string.IsNullOrEmpty(origin) || !string.IsNullOrEmpty(host)
                )
            {
                _logger.LogInformation("{0} {1} called from Origin {2} Host {3} associated with {4}", DateTime.UtcNow, methodName, origin, host, noteRequestId);
            }
            else
            {
                _logger.LogInformation("{0} {1} called from unknown Origin and Host associated with {2}", DateTime.UtcNow, methodName, noteRequestId);
            }

            return noteRequestId;

        }

        /// <summary>
        /// Use StackTrace and Reflection to get a class and method name.
        /// Frequently returning values like ".MoveNext" due to ASP NET CORE Kestrel patterns.
        /// </summary>
        /// <returns>Name of class.method</returns>
        public static string IdentifyCallingClassAndMethod()
        {

            // Be able to name the calling class and method in logs
            StringBuilder sbCallingMethod = new StringBuilder();
            StringBuilder sbCallingClass = new StringBuilder();
            StringBuilder sbMethodName = new StringBuilder();

            // Use Reflection to identify which class and method called this class and method.
            StackTrace st = new StackTrace();
            StackFrame? frame = st.GetFrame(1);
            if (frame != null)
            {
                var callingMethod = frame.GetMethod();
                if (callingMethod != null)
                {
                    var callingClass = callingMethod.DeclaringType;


                    sbCallingMethod.Append(callingMethod.Name);
                    if (callingClass != null)
                    {
                        if (callingClass.Name.Contains("<"))
                        {
                            sbCallingClass.Append(callingClass.Name.Substring(0, callingClass.Name.IndexOf("<")));
                        }
                        else
                        {
                            sbCallingClass.Append(callingClass.Name);
                        }


                    }
                    else
                    {
                        sbCallingClass.Append("undetermined Class");
                    }
                }
                else
                {
                    sbCallingMethod.Append("undetermined Method");
                }

                sbMethodName.Append(sbCallingClass.ToString());
                sbMethodName.Append('.');
                sbMethodName.Append(sbCallingMethod.ToString());
            }

            return sbMethodName.ToString();
        }


        /// <summary>
        /// Code to log the status of data processing in the application.
        /// </summary>
        /// <param name="request">HTTP Request object of the initially received API request.</param>
        /// <param name="_logger">Logger used by the class.</param>
        /// <param name="methodName">Function from which the logger should be called.</param>
        /// <param name="noteRequestId">GUID to distinctly identify the Note to be acted upon.</param>
        /// <param name="status">Success, Failure, or other DataProcessingStatus</param>
        /// <param name="cause">String describing the circumstances of success, failure, or other logged event.</param>
        /// <returns>noteRequestId that was logged.</returns>
        public static string LogDataProcessingStatusRequest(this HttpRequest request, ILogger _logger, string? methodName, string noteRequestId, DataProcessingStatus status, string cause)
        {

            _logger.LogInformation("{0} {1} request {2} {3} due to {4}.", DateTime.UtcNow, methodName, noteRequestId, status, cause);

            return noteRequestId;

        }

        /// <summary>
        /// Code to log the status of a service's work in the application.  Assumes no direct HTTP REQUEST to the function associated with this logging.
        /// </summary>
        /// <param name="_logger">Logger used by the class.</param>
        /// <param name="methodName">Function from which the logger should be called.</param>
        /// <param name="noteRequestId">GUID to distinctly identify the Note to be acted upon.</param>
        /// <param name="status">Success, Failure, or other DataProcessingStatus</param>
        /// <param name="cause">String describing the circumstances of success, failure, or other logged event.</param>
        /// <returns>noteRequestId that was logged.</returns>
        public static string LogDataProcessingStatusServiceWork(ILogger _logger, string? methodName, string noteRequestId, DataProcessingStatus status, string cause)
        {

            _logger.LogInformation("{0} {1} request {2} {3} due to {4}.", DateTime.UtcNow, methodName, noteRequestId, status, cause);

            return noteRequestId;

        }

        /// <summary>
        /// Code to log the status of a service's work in the application.  Assumes no direct HTTP REQUEST to the function associated with this logging.
        /// </summary>
        /// <param name="_logger">Logger used by the class.</param>
        /// <param name="noteRequestId">GUID to distinctly identify the Note to be acted upon.</param>
        /// <param name="status">Success, Failure, or other DataProcessingStatus</param>
        /// <param name="cause">String describing the circumstances of success, failure, or other logged event.</param>
        /// <returns>noteRequestId that was logged.</returns>
        public static string LogDataProcessingStatusServiceWork(ILogger _logger, string noteRequestId, DataProcessingStatus status, string cause)
        {

            _logger.LogInformation("{0} request {1} {2} due to {3}.", DateTime.UtcNow, noteRequestId, status, cause);

            return noteRequestId;

        }

        /// <summary>
        /// Overload without noteRequestId - logs with timestamp
        /// </summary>
        public static void LogDataProcessingStatusServiceWorkSimple(
            ILogger logger,
            string caller,
            DataProcessingStatus status,
            string message)
        {
            logger.LogInformation("{0} {1} {2} - {3}", DateTime.UtcNow, caller, status, message);
        }

        /// <summary>
        /// Overload with context parameter - logs with timestamp and contextual information
        /// </summary>
        public static void LogDataProcessingStatusWithContext(
            ILogger logger,
            string caller,
            string context,
            DataProcessingStatus status,
            string message)
        {
            var contextInfo = string.IsNullOrEmpty(context) ? string.Empty : $"[{context}] ";
            logger.LogInformation("{0} {1} {2}{3} - {4}", DateTime.UtcNow, caller, contextInfo, status, message);
        }
    }
}
