using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Mercurial.Attributes;

namespace Mercurial
{
    /// <summary>
    /// This class implements the "hg rollback" command (<see href="http://www.selenic.com/mercurial/hg.1.html#rollback"/>):
    /// Roll back the last transaction (dangerous.)
    /// </summary>
    public sealed class RollbackCommand : MercurialCommandBase<RollbackCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RollbackCommand"/> class.
        /// </summary>
        public RollbackCommand()
            : base("rollback")
        {
            // Do nothing here
        }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore safety measures.
        /// Default is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// Note that this option did not exist before Mercurial 2.0, and safety measures were ignored
        /// either way. This means that whether you set this property or not, to <c>true</c> or <c>false</c>,
        /// on a pre-2.0 client, it makes no difference, safety measures will be ignored either way.
        /// </remarks>
        [DefaultValue(false)]
        public bool Force
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the <see cref="Force"/> property to the specified value and
        /// returns this <see cref="RollbackCommand"/> instance.
        /// </summary>
        /// <param name="value">
        /// The new value for the <see cref="Force"/> property,
        /// defaults to <c>true</c>.
        /// </param>
        /// <returns>
        /// This <see cref="RollbackCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method is part of the fluent interface.
        /// </remarks>
        public RollbackCommand WithForce(bool value = true)
        {
            Force = value;
            return this;
        }

        /// <summary>
        /// Gets all the arguments to the <see cref="CommandBase{T}.Command"/>, or an
        /// empty array if there are none.
        /// </summary>
        public override IEnumerable<string> Arguments
        {
            get
            {
                if (ClientExecutable.CurrentVersion < new Version(2, 0))
                    return base.Arguments;

                if (!Force)
                    return base.Arguments;

                return base.Arguments.Concat(new[] { "--force" }).ToArray();
            }
        }
    }
}