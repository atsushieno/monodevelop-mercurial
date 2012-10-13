using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mercurial.Attributes;

namespace Mercurial
{
    /// <summary>
    /// This class implements the "hg annotate" command (<see href="http://www.selenic.com/mercurial/hg.1.html#annotate"/>):
    /// show changeset information by line for each file.
    /// </summary>
    public sealed class AnnotateCommand : MercurialCommandBase<AnnotateCommand>, IMercurialCommand<IEnumerable<Annotation>>
    {
        /// <summary>
        /// This is the backing field for the <see cref="Path"/> property.
        /// </summary>
        private string _Path = string.Empty;

		/// <summary>
		/// This is the backing field for the <see cref="AddDate"/> property.
		/// </summary>
		private bool _AddDate = false;
		
		/// <summary>
		/// This is the backing field for the <see cref="AddUserName"/> property.
		/// </summary>
		private bool _AddUserName = false;

        /// <summary>
        /// This is the backing field for the <see cref="FollowRenamesAndMoves"/> property.
        /// </summary>
        private bool _FollowRenamesAndMoves = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotateCommand"/> class.
        /// </summary>
        public AnnotateCommand()
            : base("annotate")
        {
            // Do nothing here
        }

        /// <summary>
        /// Gets or sets the path to the item to annotate.
        /// </summary>
        [NullableArgument]
        [DefaultValue("")]
        public string Path
        {
            get
            {
                return _Path;
            }

            set
            {
                _Path = (value ?? string.Empty).Trim();
            }
        }

		/// <summary>
		/// Gets or sets a value indicating whether to add date information to the resutls.
		/// Default is <c>false</c>.
		/// </summary>
		[BooleanArgument(TrueOption = "-d")]
		[DefaultValue(false)]
		public bool AddDate
		{
			get
			{
				return _AddDate;
			}
			
			set
			{
				_AddDate = value;
			}
		}

		/// <summary>
		/// Sets the <see cref="AddDate"/> property to the specified value and
		/// returns this <see cref="AnnotateCommand"/> instance.
		/// </summary>
		/// <param name="value">
		/// The new value for the <see cref="AddDate"/> property,
		/// defaults to <c>true</c>.
		/// </param>
		/// <returns>
		/// This <see cref="AnnotateCommand"/> instance.
		/// </returns>
		/// <remarks>
		/// This method is part of the fluent interface.
		/// </remarks>
		public AnnotateCommand WithAddDate(bool value = true)
		{
			AddDate = value;
			return this;
		}

		/// <summary>
		/// Gets or sets a value indicating whether to add user information to the resutls.
		/// Default is <c>false</c>.
		/// </summary>
		[BooleanArgument(TrueOption = "-u")]
		[DefaultValue(false)]
		public bool AddUserName
		{
			get
			{
				return _AddUserName;
			}
			
			set
			{
				_AddUserName = value;
			}
		}

		/// <summary>
		/// Sets the <see cref="AddUserName"/> property to the specified value and
		/// returns this <see cref="AnnotateCommand"/> instance.
		/// </summary>
		/// <param name="value">
		/// The new value for the <see cref="AddUserName"/> property,
		/// defaults to <c>true</c>.
		/// </param>
		/// <returns>
		/// This <see cref="AnnotateCommand"/> instance.
		/// </returns>
		/// <remarks>
		/// This method is part of the fluent interface.
		/// </remarks>
		public AnnotateCommand WithAddUserName(bool value = true)
		{
			AddUserName = value;
			return this;
		}

		/// <summary>
        /// Gets or sets a value indicating whether to follow renames and copies when limiting the log.
        /// Default is <c>false</c>.
        /// </summary>
        [BooleanArgument(FalseOption = "--no-follow")]
        [DefaultValue(true)]
        public bool FollowRenamesAndMoves
        {
            get
            {
                return _FollowRenamesAndMoves;
            }

            set
            {
                _FollowRenamesAndMoves = value;
            }
        }

        /// <summary>
        /// Gets or sets which revision of the specified item to annotate.
        /// If <c>null</c>, annotate the revision in the working folder.
        /// Default is <c>null</c>.
        /// </summary>
        [NullableArgument(NonNullOption = "--rev")]
        [DefaultValue(null)]
        public RevSpec Revision
        {
            get;
            set;
        }

        #region IMercurialCommand<IEnumerable<Annotation>> Members

        /// <summary>
        /// Validates the command configuration. This method should throw the necessary
        /// exceptions to signal missing or incorrect configuration (like attempting to
        /// add files to the repository without specifying which files to add.)
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The 'annotate' command requires Path to be set.
        /// </exception>
        public override void Validate()
        {
            base.Validate();

            if (StringEx.IsNullOrWhiteSpace(Path))
                throw new InvalidOperationException("The 'annotate' command requires Path to be set");
        }

        /// <summary>
        /// Gets the result of executing the command as a collection of <see cref="Annotation"/> objects.
        /// </summary>
        public IEnumerable<Annotation> Result
        {
            get;
            private set;
        }

        #endregion

        /// <summary>
        /// Sets the <see cref="Path"/> property to the specified value and
        /// returns this <see cref="AnnotateCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="Path"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="AnnotateCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public AnnotateCommand WithPath(string value)
        {
            Path = value;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="FollowRenamesAndMoves"/> property to the specified value and
        /// returns this <see cref="AnnotateCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="FollowRenamesAndMoves"/> property,
        /// defaults to <c>true</c>.
        /// </param>
        /// <returns>
        /// This <see cref="AnnotateCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public AnnotateCommand WithFollowRenamesAndMoves(bool value = true)
        {
            FollowRenamesAndMoves = value;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="Revision"/> property to the specified value and
        /// returns this <see cref="AnnotateCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="Revision"/> property.
        /// </param>
        /// <returns>
        /// This <see cref="AnnotateCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public AnnotateCommand WithRevision(RevSpec value)
        {
            Revision = value;
            return this;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore whitespace when comparing lines.
        /// </summary>
        [BooleanArgument(TrueOption = "--ignore-all-space")]
        [DefaultValue(false)]
        public bool IgnoreWhiteSpace
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the <see cref="IgnoreWhiteSpace"/> property to the specified value and
        /// returns this <see cref="AnnotateCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="IgnoreWhiteSpace"/> property,
        /// defaults to <c>true</c>.
        /// </param>
        /// <returns>
        /// This <see cref="AnnotateCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public AnnotateCommand WithIgnoreWhiteSpace(bool value = true)
        {
            IgnoreWhiteSpace = value;
            return this;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore changes in the amount of whitespace when comparing lines.
        /// </summary>
        [BooleanArgument(TrueOption = "--ignore-space-change")]
        [DefaultValue(false)]
        public bool IgnoreWhiteSpaceChange
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the <see cref="IgnoreWhiteSpaceChange"/> property to the specified value and
        /// returns this <see cref="AnnotateCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="IgnoreWhiteSpaceChange"/> property,
        /// defaults to <c>true</c>.
        /// </param>
        /// <returns>
        /// This <see cref="AnnotateCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public AnnotateCommand WithIgnoreWhiteSpaceChange(bool value = true)
        {
            IgnoreWhiteSpaceChange = value;
            return this;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore blank lines when comparing lines.
        /// </summary>
        [BooleanArgument(TrueOption = "--ignore-blank-lines")]
        [DefaultValue(false)]
        public bool IgnoreBlankLines
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the <see cref="IgnoreBlankLines"/> property to the specified value and
        /// returns this <see cref="AnnotateCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="IgnoreBlankLines"/> property,
        /// defaults to <c>true</c>.
        /// </param>
        /// <returns>
        /// This <see cref="AnnotateCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public AnnotateCommand WithIgnoreBlankLines(bool value = true)
        {
            IgnoreBlankLines = value;
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
        protected override void ParseStandardOutputForResults(int exitCode, string standardOutput)
        {
            base.ParseStandardOutputForResults(exitCode, standardOutput);

            var result = new List<Annotation>();
			var re = new Regex(@"^(?<rev>\d+): (?<line>.*)$", RegexOptions.None);
			var red = new Regex(@"\w\w\w \w\w\w \d\d \d\d:\d\d:\d\d \d\d\d\d [+-]\d\d\d\d:", RegexOptions.None);

			using (var reader = new StringReader(standardOutput))
            {
                string line;
                int lineNumber = 0;
				try {
                while ((line = reader.ReadLine()) != null)
                {
					DateTimeOffset date = DateTimeOffset.MaxValue;
					string user = String.Empty;
					if (AddDate) {
						Match mad = red.Match(line);
						if (mad.Success) {
							var dtstr = line.Substring (mad.Index, 28);
							// huh, DateTimeOffset is not capable of parsing timezone +HHmm. I had to explicitly add ':' !
							date = DateTimeOffset.ParseExact (dtstr + ':' + line.Substring (mad.Index + 28, 2), "ddd MMM dd HH:mm:ss yyyy zzz", CultureInfo.InvariantCulture);
							line = line.Substring(0, mad.Index - 1).Trim () + line.Substring(mad.Index + 30);
						}
					}
					if (AddUserName) {
						int idx = line.IndexOf(':');
						user = line.Substring(0, line.LastIndexOf(' ', idx)).Trim ();
						var ddd = line;
						line = line.Substring(user.Length + 1);
						TextWriter.Null.WriteLine (ddd);
					}
                    Match ma = re.Match(line);
                    if (ma.Success) {
						var rev = ma.Groups["rev"].Value;
						var lineg = ma.Groups["line"].Value;
                        result.Add(
                            new Annotation(lineNumber, int.Parse(rev, CultureInfo.InvariantCulture), line, user, date));
					}
                    lineNumber++;
                }
				} catch (Exception ) { throw; }
            }
            Result = result;
        }

		public override IEnumerable<string> Arguments {
			get {
				return base.Arguments.Concat (new string [] {"-n"});
			}
		}
    }
}