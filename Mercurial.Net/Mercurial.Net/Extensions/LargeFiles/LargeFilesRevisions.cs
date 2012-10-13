namespace Mercurial.Extensions.LargeFiles
{
    /// <summary>
    /// This enum is used by the <see cref="LargeFilesVerifyCommandExtensions"/>, to specify which revisions
    /// of the large files repository to verify during the large files repository verification process.
    /// </summary>
    public enum LargeFilesRevisions
    {
        /// <summary>
        /// Only verify the large files for the current revision.
        /// </summary>
        Current,

        /// <summary>
        /// Verify the large files of all the revisions.
        /// </summary>
        AllRevisions,
    }
}