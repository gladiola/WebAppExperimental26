namespace REDRFID.Interfaces.Main_Objects
{
    /// <summary>
    /// Be able to provide copies of blob connection strings.
    /// </summary>
    public interface IBlobSettingsService
    {
        /// <summary>
        /// Be able to report a blob's connection string.
        /// </summary>
        string? BlobConnectionString { get; }

        /// <summary>
        /// Limit the number of items in a blob's container
        /// </summary>
        int? MaxAttachments { get; }
    }
}