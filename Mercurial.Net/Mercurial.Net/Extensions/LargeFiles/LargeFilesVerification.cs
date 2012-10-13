using System;

namespace Mercurial.Extensions.LargeFiles
{
    /// <summary>
    /// This enum is used by the <see cref="LargeFilesVerifyCommandExtensions"/> to specify what to verify
    /// of the large files repository.
    /// </summary>
    [Flags]
    public enum LargeFilesVerification
    {
        /// <summary>
        /// Verify the existance of the large files.
        /// </summary>
        Existance = 1,

        /// <summary>
        /// Verify the existance and contents of the large files.
        /// </summary>
        Content = 2
    }
}