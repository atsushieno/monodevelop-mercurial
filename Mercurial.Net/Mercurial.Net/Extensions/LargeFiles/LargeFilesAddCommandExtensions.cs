using System;
using System.Globalization;

namespace Mercurial.Extensions.LargeFiles
{
    /// <summary>
    /// This class adds extension methods to the <see cref="AddCommand"/> class, for
    /// the Mercurial LargeFiles extension.
    /// </summary>
    public static class LargeFilesAddCommandExtensions
    {
        /// <summary>
        /// Specify that the files that are added should be added as normal files (ie. not as "large files".)
        /// </summary>
        /// <param name="command">
        /// The <see cref="AddCommand"/> to modify.
        /// </param>
        /// <returns>
        /// The <see cref="AddCommand"/>, for a fluent interface.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="command"/> is <c>null</c>.
        /// </exception>
        public static AddCommand WithAddAsNormalFile(this AddCommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            command.AddArgument("--normal");
            return command;
        }

        /// <summary>
        /// Specify that the files that are added should be added as large files.
        /// </summary>
        /// <param name="command">
        /// The <see cref="AddCommand"/> to modify.
        /// </param>
        /// <returns>
        /// The <see cref="AddCommand"/>, for a fluent interface.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="command"/> is <c>null</c>.
        /// </exception>
        public static AddCommand WithAddAsLargeFile(this AddCommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            command.AddArgument("--large");
            return command;
        }

        /// <summary>
        /// Specify that the files that are added should be added as large files if they're above a specific
        /// size, in megabytes, otherwise they should be added as normal files.
        /// </summary>
        /// <param name="command">
        /// The <see cref="AddCommand"/> to modify.
        /// </param>
        /// <param name="size">
        /// The size threshold, in megabytes, of which files above this size should be added as large files.
        /// </param>
        /// <returns>
        /// The <see cref="AddCommand"/>, for a fluent interface.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="command"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="size"/> is less than 1.
        /// </exception>
        public static AddCommand WithAddAllFilesAboveSizeAsLargeFiles(this AddCommand command, int size)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (size < 1)
                throw new ArgumentOutOfRangeException("size", size, "size must be 1 or higher");

            command.AddArgument("--lfsize");
            command.AddArgument(size.ToString(CultureInfo.InvariantCulture));
            return command;
        }
    }
}
