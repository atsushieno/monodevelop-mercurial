using System;
using System.ComponentModel;
using Mercurial.Attributes;

namespace Mercurial
{
    /// <summary>
    /// This class implements the "hg graft" command (<see href="http://www.selenic.com/mercurial/hg.1.html#graft"/>):
    /// copy changes from other branches onto the current branch.
    /// </summary>
    public sealed class GraftCommand : MercurialCommandBase<GraftCommand>
    {
        /// <summary>
        /// This is the backing field for the <see cref="OverrideAuthor"/> property.
        /// </summary>
        private string _OverrideAuthor = string.Empty;

        /// <summary>
        /// This is the backing field for the <see cref="MergeTool"/> property.
        /// </summary>
        private string _MergeTool = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraftCommand"/> class.
        /// </summary>
        public GraftCommand()
            : base("graft")
        {
            RequiresVersion(new Version(2, 0), "GraftCommand");
        }

        /// <summary>
        /// Gets or sets the <see cref="RevSpec"/> of the changeset(s) to graft unto the current branch.
        /// </summary>
        [NullableArgument]
        [DefaultValue(null)]
        public RevSpec Revision
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the <see cref="Revision"/> property to the specified value and
        /// returns this <see cref="GraftCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="Revision"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="GraftCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public GraftCommand WithRevision(RevSpec value)
        {
            Revision = value;
            return this;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to continue an interrupt graft.
        /// </summary>
        [BooleanArgument(TrueOption = "--continue")]
        [DefaultValue(false)]
        public bool ContinueInterruptedGraft
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the <see cref="ContinueInterruptedGraft"/> property to the specified value and
        /// returns this <see cref="GraftCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="ContinueInterruptedGraft"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="GraftCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public GraftCommand WithContinueInterruptedGraft(bool value)
        {
            ContinueInterruptedGraft = value;
            return this;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use the current date and time instead
        /// of the timestamp of the original changeset being grafted.
        /// </summary>
        [BooleanArgument(TrueOption = "--currentdate")]
        [DefaultValue(false)]
        public bool UseCurrentTimestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the <see cref="UseCurrentTimestamp"/> property to the specified value and
        /// returns this <see cref="GraftCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="UseCurrentTimestamp"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="GraftCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public GraftCommand WithUseCurrentTimestamp(bool value)
        {
            UseCurrentTimestamp = value;
            return this;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use the current Mercurial author
        /// instead of the username of the original changeset being grafted.
        /// </summary>
        [BooleanArgument(TrueOption = "--currentuser")]
        [DefaultValue(false)]
        public bool UseCurrentAuthor
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the <see cref="UseCurrentAuthor"/> property to the specified value and
        /// returns this <see cref="GraftCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="UseCurrentAuthor"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="GraftCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public GraftCommand WithUseCurrentAuthor(bool value)
        {
            UseCurrentAuthor = value;
            return this;
        }

        /// <summary>
        /// Gets or sets the username to use when committing;
        /// or <see cref="string.Empty"/> to use the username configured in the repository or by
        /// the current user. Default is <see cref="string.Empty"/>.
        /// </summary>
        [NullableArgument(NonNullOption = "--user")]
        [DefaultValue("")]
        public string OverrideAuthor
        {
            get
            {
                return _OverrideAuthor;
            }

            set
            {
                _OverrideAuthor = (value ?? string.Empty).Trim();
            }
        }

        /// <summary>
        /// Sets the <see cref="OverrideAuthor"/> property to the specified value and
        /// returns this <see cref="GraftCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="OverrideAuthor"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="GraftCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public GraftCommand WithOverrideAuthor(string value)
        {
            OverrideAuthor = value;
            return this;
        }

        /// <summary>
        /// Gets or sets the timestamp <see cref="DateTime"/> to use when committing;
        /// or <c>null</c> which means use the current date and time. Default is <c>null</c>.
        /// </summary>
        [DateTimeArgument(NonNullOption = "--date")]
        [DefaultValue(null)]
        public DateTime? OverrideTimestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the <see cref="OverrideTimestamp"/> property to the specified value and
        /// returns this <see cref="GraftCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="OverrideTimestamp"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="GraftCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public GraftCommand WithOverrideTimestamp(DateTime value)
        {
            OverrideTimestamp = value;
            return this;
        }

        /// <summary>
        /// Gets or sets the merge tool to use.
        /// Default value is <see cref="string.Empty"/> in which case the default merge tool(s) are used.
        /// </summary>
        [DefaultValue("")]
        public string MergeTool
        {
            get
            {
                return _MergeTool;
            }

            set
            {
                _MergeTool = (value ?? string.Empty).Trim();
            }
        }

        /// <summary>
        /// Sets the <see cref="MergeTool"/> property to the specified value and
        /// returns this <see cref="GraftCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="MergeTool"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="GraftCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public GraftCommand WithMergeTool(string value)
        {
            MergeTool = value;
            return this;
        }

        /// <summary>
        /// Validates the command configuration. This method should throw the necessary
        /// exceptions to signal missing or incorrect configuration (like attempting to
        /// add files to the repository without specifying which files to add.)
        /// </summary>
        /// <remarks>
        /// Note that as long as you descend from <see cref="MercurialCommandBase{T}"/> you're not required to call
        /// the base method at all.
        /// </remarks>
        public override void Validate()
        {
            base.Validate();

            if (Revision == null)
                throw new InvalidOperationException("The Revision property must be set on the GraftCommand before executing it");
        }
    }
}
