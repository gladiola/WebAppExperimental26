using REDRFID.Interfaces.Main_Objects;
using REDRFID.Models.Settings;

namespace REDRFID.Services
{
    /// <summary>
    /// Polymorph chain of NoteSettingsService
    /// </summary>
    public class BlobSettingsService : BlobSettings, IBlobSettingsService
    {
        /// <summary>
        /// Make a connection string to blob storage available to other classes.
        /// </summary>
        /// <param name="bs">BlobSettings object that holds a connection string.</param>
        public BlobSettingsService(BlobSettings bs) {
            BlobConnectionString = bs.BlobConnectionString;
            MaxAttachments = bs.MaxAttachments;
        }

    }
}
