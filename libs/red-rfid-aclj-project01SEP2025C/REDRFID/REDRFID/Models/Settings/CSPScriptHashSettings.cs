namespace REDRFID.Models.Settings
{
    public class CSPScriptHashSettings
    {


        public CSPScriptHashSettings() { }

        public required string CSPScriptHashFilePath { get; set; }
        public required string ManuallyCalculatedInlineHash1 { get; set; }
    }
}
