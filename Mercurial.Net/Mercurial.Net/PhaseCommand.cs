using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Mercurial.Attributes;

namespace Mercurial
{
    /// <summary>
    /// This class implements the "hg cat" command (<see href="http://www.selenic.com/mercurial/hg.1.html#phase"/>):
    /// set or show the current phase name.
    /// </summary>
    public sealed class PhaseCommand : MercurialCommandBase<PhaseCommand>, IMercurialCommand<IEnumerable<ChangesetPhase>> 
    {
        /// <summary>
        /// This is the backing field for the <see cref="Revisions"/> property.
        /// </summary>
        private readonly List<RevSpec> _Revisions = new List<RevSpec>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PhaseCommand"/> class.
        /// </summary>
        public PhaseCommand()
            : base("phase")
        {
            RequiresVersion(new Version(2, 1), "PhaseCommand");
        }

        /// <summary>
        /// Gets or sets a value indicating which phase to set the changesets to, or whether to retrieve
        /// the current phases of the changesets.
        /// Default is <see cref="Phases.List"/>.
        /// </summary>
        [EnumArgument(Phases.Draft, "--draft")]
        [EnumArgument(Phases.Public, "--public")]
        [EnumArgument(Phases.Secret, "--secret")]
        [DefaultValue(Phases.List)]
        public Phases Phase
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the <see cref="Phase"/> property to the specified value and
        /// returns this <see cref="PhaseCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="Phase"/> property,
        /// defaults to <c>100</c>.
        /// </param>
        /// <returns>
        /// This <see cref="PhaseCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public PhaseCommand WithPhase(Phases value)
        {
            Phase = value;
            return this;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to force moving a phase boundary backward.
        /// </summary>
        [BooleanArgument(TrueOption = "--force")]
        [DefaultValue(false)]
        public bool Force
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the <see cref="Force"/> property to the specified value and
        /// returns this <see cref="PhaseCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="Force"/> property,
        /// defaults to <c>true</c>.
        /// </param>
        /// <returns>
        /// This <see cref="PhaseCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public PhaseCommand WithForce(bool value = true)
        {
            Force = value;
            return this;
        }

        /// <summary>
        /// Gets the collection <see cref="RevSpec"/> that identifies the revision(s) or the
        /// revision range(s) to list or set the phase of.
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
        /// Adds the specified value to the <see cref="Revisions"/> property and
        /// returns this <see cref="PhaseCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The value to add to the <see cref="Revisions"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="PhaseCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public PhaseCommand WithRevisions(RevSpec value)
        {
            Revisions.Add(value);
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
            if (exitCode != 0)
                Result = new ChangesetPhase[0];
            else
            {
                var re = new Regex(@"^(?<revnum>\d+):\s+(?<phase>secret|public|draft)\s*", RegexOptions.IgnoreCase);
                var result = new List<ChangesetPhase>();
                using (var reader = new StringReader(RawStandardOutput))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var ma = re.Match(line);
                        if (ma.Success)
                        {
                            int revnum = int.Parse(ma.Groups["revnum"].Value, CultureInfo.InvariantCulture);
                            Phases phase;
                            switch (ma.Groups["phase"].Value.ToLowerInvariant())
                            {
                                case "draft":
                                    phase = Phases.Draft;
                                    break;

                                case "public":
                                    phase = Phases.Public;
                                    break;

                                case "secret":
                                    phase = Phases.Secret;
                                    break;

                                default:
                                    throw new InvalidOperationException("Internal error; unknown phase retrieved from 'hg phase'");
                            }
                            result.Add(new ChangesetPhase(revnum, phase));
                        }
                    }
                }
                Result = result;
            }
            base.ParseStandardOutputForResults(exitCode, standardOutput);
        }

        /// <summary>
        /// Gets the result from the command line execution, as an appropriately typed value.
        /// </summary>
        public IEnumerable<ChangesetPhase> Result
        {
            get;
            private set;
        }
    }
}
