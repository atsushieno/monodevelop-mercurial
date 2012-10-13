using System;

namespace Mercurial.Extensions.LargeFiles
{
    /// <summary>
    /// This class adds extension methods to the <see cref="VerifyCommand"/> class, for
    /// the Mercurial LargeFiles extension.
    /// </summary>
    public static class LargeFilesVerifyCommandExtensions
    {
        /// <summary>
        /// Instruct the verify command to also verify the contents of the large files repository.
        /// </summary>
        /// <param name="command">
        /// The <see cref="VerifyCommand"/> to modify.
        /// </param>
        /// <param name="revision">
        /// The revision to verify.
        /// Defaults to <see cref="LargeFilesRevisions.Current"/>.
        /// </param>
        /// <param name="verify">
        /// What to verify.
        /// Defaults to <see cref="LargeFilesVerification.Existance"/>.
        /// </param>
        /// <returns>
        /// The <see cref="VerifyCommand"/>, for a fluent interface.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="command"/> is <c>null</c>.
        /// </exception>
        public static VerifyCommand WithVerifyLargeFiles(this VerifyCommand command, LargeFilesRevisions revision = LargeFilesRevisions.Current, LargeFilesVerification verify = LargeFilesVerification.Existance)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            command.AddArgument("--large");

            switch (revision)
            {
                case LargeFilesRevisions.Current:
                    break;

                case LargeFilesRevisions.AllRevisions:
                    command.AddArgument("--lfa");
                    break;
            }

            switch (verify)
            {
                case LargeFilesVerification.Existance:
                    break;

                case LargeFilesVerification.Content:
                    command.AddArgument("--lfc");
                    break;
            }

            return command;
        }
    }
}
