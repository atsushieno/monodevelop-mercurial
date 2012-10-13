namespace Mercurial.Extensions.LargeFiles
{
    /// <summary>
    /// This class contains logic for the Mercurial LargeFiles extension.
    /// </summary>
    public class LargeFilesExtension
    {
        /// <summary>
        /// Gets a value indicating whether the Mercurial LargeFiles extension is installed and active.
        /// </summary>
        public static bool IsInstalled
        {
            get
            {
                return ClientExecutable.Configuration.ValueExists("extensions", "largefiles");
            }
        }
    }
}
