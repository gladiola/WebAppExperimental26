namespace REDRFID.Models.Main_Objects
{

    // REF:  https://josipmisko.com/posts/string-enums-in-c-sharp-everything-you-need-to-know

    // Used in conjunction with Enumeration abstract class from 
    // Microsoft docs, quoted verbatim.

    /// <summary>
    /// Code to limit logging message words when logging the status of data processing in the API.  Acts as enumeration of strings.
    /// </summary>
    public class DataProcessingStatus : Enumeration
    {

        /// <summary>
        /// Word used to describe success in status messages.
        /// </summary>
        public static DataProcessingStatus Success => new(1, "success");

        /// <summary>
        /// Word used to describe failure in status messages.
        /// </summary>
        public static DataProcessingStatus Failure => new(2, "failure");

        /// <summary>
        /// Word used to describe an informational message.
        /// </summary>
        public static DataProcessingStatus Info => new(3, "INFO");

        /// <summary>
        /// Word used to describe errors in status messages.
        /// </summary>
        public static DataProcessingStatus Error => new(4, "error");

        /// <summary>
        /// Word used to describe exceptions in status messages.
        /// </summary>
        public static DataProcessingStatus Exception => new(5, "exception");

        /// <summary>
        /// Code to enumerate over strings.
        /// </summary>
        /// <param name="id">Number in enumeration.</param>
        /// <param name="name">String associated with id.</param>
        public DataProcessingStatus(int id, string name)
    : base(id, name)
        {
        }


    }

}
