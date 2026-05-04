using REDRFID.Models.Main_Objects;

namespace REDRFID.Services
{
    public interface INonceCatalogService {

        public bool AddANonce(string whichOne, Nonce nonce);
        public string GetANonce(string whichOne);
        public bool RemoveANonce(string whichOne);

    }


    /// <summary>
    /// Utility class to generate and hold nonces in key-value pair dictionary.
    /// </summary>
    public class NonceCatalogService : INonceCatalogService
    {
        // In-memory storage for the nonces
        private static readonly Dictionary<string, Nonce> _nonceCollection = new Dictionary<string, Nonce>();

        // Logging for the service
        private readonly ILogger<NonceCatalogService> _logger ;

        // add a nonce by given name to a dictionary
        // retrieve a nonce by a given name from a dictionary

        public NonceCatalogService(ILogger<NonceCatalogService> logger) { 
        
            _logger = logger;

        }

        /// <summary>
        /// Add a Nonce object which should contain a generated value to the collection.
        /// </summary>
        /// <param name="whichOne"></param>
        /// <param name="nonce"></param>
        /// <returns></returns>
        public bool AddANonce(string whichOne, Nonce nonce)
        {
            _nonceCollection[whichOne] = nonce;
            return _nonceCollection.TryGetValue(whichOne, out _);
        }

        /// <summary>
        /// Get a Nonce object by a known key
        /// </summary>
        /// <param name="whichOne"></param>
        /// <returns>string representation of the nonce value</returns>
        public string GetANonce(string whichOne)
        {
            string answer = String.Empty;

            if (_nonceCollection.TryGetValue(whichOne, out _)) {
                Nonce nonce = _nonceCollection[whichOne];
                answer = nonce.GetNonceAsString();
            }
                 
            return answer;
        }

        /// <summary>
        /// Remove a Nonce object by a known key.
        /// </summary>
        /// <param name="whichOne"></param>
        /// <returns>True if the collection no longer holds that key.</returns>
        public bool RemoveANonce(string whichOne)
        {
            bool answer = false;

            if (_nonceCollection.TryGetValue(whichOne, out _))
            {
                _nonceCollection.Remove(whichOne);
                answer = !_nonceCollection.ContainsKey(whichOne);
            }

            return answer;
        }
    }
}
