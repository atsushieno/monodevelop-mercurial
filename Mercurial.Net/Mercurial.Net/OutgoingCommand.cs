using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Mercurial.Attributes;

namespace Mercurial
{
    /// <summary>
    /// This class implements the "hg outgoing" command (<see href="http://www.selenic.com/mercurial/hg.1.html#outgoing"/>):
    /// show changesets not found in the destination.
    /// </summary>
    public sealed class OutgoingCommand : MercurialCommandBase<OutgoingCommand>, IMercurialCommand<IEnumerable<Changeset>>
    {
        /// <summary>
        /// This is the backing field for the <see cref="Revisions"/> property.
        /// </summary>
        private readonly List<RevSpec> _Revisions = new List<RevSpec>();

        /// <summary>
        /// This is the backing field for the <see cref="Destination"/> property.
        /// </summary>
        private string _Destination = string.Empty;

        /// <summary>
        /// This is the backing field for the <see cref="Branch"/> property.
        /// </summary>
        private string _Branch = string.Empty;
		
		/// <summary>
        /// This is the backing field for the <see cref="SshCommand"/> property.
        /// </summary>
        private string _SshCommand = string.Empty;

        /// <summary>
        /// This is the backing field for the <see cref="RemoteCommand"/> property.
        /// </summary>
        private string _RemoteCommand = string.Empty;

        /// <summary>
        /// This is the backing field for the <see cref="VerifyServerCertificate"/> property.
        /// </summary>
        private bool _VerifyServerCertificate = true;

        /// <summary>
        /// This is the backing field for the <see cref="RecurseSubRepositories"/> property.
        /// </summary>
        private bool _RecurseSubRepositories;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutgoingCommand"/> class.
        /// </summary>
        public OutgoingCommand()
            : base("outgoing")
        {
        }

        /// <summary>
        /// Gets or sets the ssh command to use when cloning.
        /// Default is <see cref="string.Empty"/>.
        /// </summary>
        [NullableArgument(NonNullOption = "--ssh")]
        [DefaultValue("")]
        public string SshCommand
        {
            get
            {
                return _SshCommand;
            }

            set
            {
                _SshCommand = (value ?? string.Empty).Trim();
            }
        }

        /// <summary>
        /// Gets or sets the hg command to run on the remote side.
        /// Default is <see cref="string.Empty"/>.
        /// </summary>
        [NullableArgument(NonNullOption = "--remotecmd")]
        [DefaultValue("")]
        public string RemoteCommand
        {
            get
            {
                return _RemoteCommand;
            }

            set
            {
                _RemoteCommand = (value ?? string.Empty).Trim();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to verify the server certificate. If set to <c>false</c>, will ignore web.cacerts configuration.
        /// Default value is <c>true</c>.
        /// </summary>
        [BooleanArgument(FalseOption = "--insecure")]
        [DefaultValue(true)]
        public bool VerifyServerCertificate
        {
            get
            {
                return _VerifyServerCertificate;
            }

            set
            {
                RequiresVersion(new Version(1, 7, 5), "VerifyServerCertificate property of the OutgoingCommand class");
                _VerifyServerCertificate = value;
            }
        }

        /// <summary>
        /// Sets the <see cref="SshCommand"/> property to the specified value and
        /// returns this <see cref="OutgoingCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="SshCommand"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="OutgoingCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public OutgoingCommand WithSshCommand(string value)
        {
            SshCommand = value;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="RemoteCommand"/> property to the specified value and
        /// returns this <see cref="OutgoingCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="RemoteCommand"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="OutgoingCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public OutgoingCommand WithRemoteCommand(string value)
        {
            RemoteCommand = value;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="VerifyServerCertificate"/> property to the specified value and
        /// returns this <see cref="OutgoingCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="VerifyServerCertificate"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="OutgoingCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public OutgoingCommand WithVerifyServerCertificate(bool value)
        {
            VerifyServerCertificate = value;
            return this;
        }

        /// <summary>
        /// Gets or sets the destination to pull from. If <see cref="string.Empty"/>, push to the
        /// default destination. Default is <see cref="string.Empty"/>.
        /// </summary>
        [NullableArgument]
        [DefaultValue("")]
        public string Destination
        {
            get
            {
                return _Destination;
            }

            set
            {
                _Destination = (value ?? string.Empty).Trim();
            }
        }

        /// <summary>
        /// Gets or sets the branch to pull from. If <see cref="string.Empty"/>, push to the
        /// default branch. Default is <see cref="string.Empty"/>.
        /// </summary>
        [NullableArgument(NonNullOption = "--branch")]
        [DefaultValue("")]
        public string Branch
        {
            get
            {
                return _Branch;
            }

            set
            {
                _Branch = (value ?? string.Empty).Trim();
            }
        }
		
        /// <summary>
        /// Gets or sets a value indicating whether to run even if the repositories are unrelated. Default is <c>false</c>.
        /// </summary>
        [BooleanArgument(TrueOption = "--force")]
        [DefaultValue(false)]
        public bool Force
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the collection of revisions intended to be included in the destination.
        /// If empty, include all changes. Default is empty.
        /// </summary>
        [RepeatableArgument(Option = "--rev")]
        public Collection<RevSpec> Revisions
        {
            get
            {
                return new Collection<RevSpec>(_Revisions);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to recurse into subrepositories.
        /// Default is <c>false</c>.
        /// </summary>
        [BooleanArgument(TrueOption = "--subrepos")]
        [DefaultValue(false)]
        public bool RecurseSubRepositories
        {
            get
            {
                return _RecurseSubRepositories;
            }

            set
            {
                RequiresVersion(new Version(1, 7), "RecurseSubRepositories property of the OutgoingCommand class");
                _RecurseSubRepositories = value;
            }
        }

        #region IMercurialCommand<IEnumerable<Changeset>> Members

        /// <summary>
        /// Gets all the arguments to the <see cref="CommandBase{T}.Command"/>, or an
        /// empty array if there are none.
        /// </summary>
        /// <value></value>
        public override IEnumerable<string> Arguments
        {
            get
            {
                var arguments = new[]
                {
                    "--style=xml", "--quiet"
                };
                return arguments.Concat(base.Arguments).ToArray();
            }
        }

        /// <summary>
        /// Gets the result of executing the command as a collection of <see cref="Changeset"/> objects.
        /// </summary>
        public IEnumerable<Changeset> Result
        {
            get;
            private set;
        }

        #endregion

        /// <summary>
        /// Sets the <see cref="Destination"/> property to the specified value and
        /// returns this <see cref="OutgoingCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="Destination"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="OutgoingCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public OutgoingCommand WithDestination(string value)
        {
            Destination = value;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="Branch"/> property to the specified value and
        /// returns this <see cref="OutgoingCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="Branch"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="OutgoingCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public OutgoingCommand WithBranch(string value)
        {
            Branch = value;
            return this;
        }
		
        /// <summary>
        /// Sets the <see cref="Force"/> property to the specified value and
        /// returns this <see cref="OutgoingCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="Force"/> property,
        /// defaults to <c>true</c>.
        /// </param>
        /// <returns>
        /// This <see cref="OutgoingCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public OutgoingCommand WithForce(bool value = true)
        {
            Force = value;
            return this;
        }

        /// <summary>
        /// Adds the value to the <see cref="Revisions"/> collection property and
        /// returns this <see cref="OutgoingCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The value to add to the <see cref="Revisions"/> collection property.
        /// </param>
        /// <returns>
        /// This <see cref="OutgoingCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public OutgoingCommand WithRevision(RevSpec value)
        {
            Revisions.Add(value);
            return this;
        }

        /// <summary>
        /// Sets the <see cref="RecurseSubRepositories"/> property to the specified value and
        /// returns this <see cref="OutgoingCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="RecurseSubRepositories"/> property,
        /// defaults to <c>true</c>.
        /// </param>
        /// <returns>
        /// This <see cref="OutgoingCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public OutgoingCommand WithRecurseSubRepositories(bool value)
        {
            RecurseSubRepositories = value;
            return this;
        }

        /// <summary>
        /// This method should parse and store the appropriate execution result output
        /// according to the type of data the command line client would return for
        /// the command.
        /// </summary>
        /// <param name="exitCode">
        /// The exit code from executing the command line client.
        /// </param>
        /// <param name="standardOutput">
        /// The standard output from executing the command line client.
        /// </param>
        /// <remarks>
        /// Note that as long as you descend from <see cref="MercurialCommandBase{T}"/> you're not required to call
        /// the base method at all.
        /// </remarks>
        protected override void ParseStandardOutputForResults(int exitCode, string standardOutput)
        {
            Result = ChangesetXmlParser.Parse(standardOutput);
        }

        /// <summary>
        /// This method should throw the appropriate exception depending on the contents of
        /// the <paramref name="exitCode"/> and <paramref name="standardErrorOutput"/>
        /// parameters, or simply return if the execution is considered successful.
        /// </summary>
        /// <param name="exitCode">
        /// The exit code from executing the command line client.
        /// </param>
        /// <param name="standardErrorOutput">
        /// The standard error output from executing the command client.
        /// </param>
        /// <remarks>
        /// Note that as long as you descend from <see cref="MercurialCommandBase{T}"/> you're not required to call
        /// the base method at all. The default behavior is to throw a <see cref="MercurialExecutionException"/>
        /// if <paramref name="exitCode"/> is not zero. If you require different behavior, don't call the base
        /// method.
        /// </remarks>
        protected override void ThrowOnUnsuccessfulExecution(int exitCode, string standardErrorOutput)
        {
            switch (exitCode)
            {
                case 0:
                case 1:
                    break;

                default:
                    base.ThrowOnUnsuccessfulExecution(exitCode, standardErrorOutput);
                    break;
            }
        }

        /// <summary>
        /// Validates the command configuration. This method should throw the necessary
        /// exceptions to signal missing or incorrect configuration (like attempting to
        /// add files to the repository without specifying which files to add.)
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <para>The <see cref="VerifyServerCertificate"/> command was used with Mercurial 1.7.4 or older.</para>
        /// </exception>
        public override void Validate()
        {
            base.Validate();

            if (!VerifyServerCertificate && ClientExecutable.CurrentVersion < new Version(1, 7, 5))
                throw new InvalidOperationException("The 'VerifyServerCertificate' property is only available in Mercurial 1.7.5 and newer");
        }
    }
}
