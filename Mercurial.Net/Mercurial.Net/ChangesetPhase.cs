using System;

namespace Mercurial
{
    /// <summary>
    /// Objects of this class is returned through <see cref="PhaseCommand.Result"/>, after listing the phases
    /// of changesets.
    /// </summary>
    public class ChangesetPhase : IEquatable<ChangesetPhase>
    {
        /// <summary>
        /// This is the backing field for the <see cref="RevisionNumber"/> property.
        /// </summary>
        private readonly int _RevisionNumber;

        /// <summary>
        /// This is the backing field for the <see cref="Phase"/> property.
        /// </summary>
        private readonly Phases _Phase;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangesetPhase"/> class.
        /// </summary>
        /// <param name="revisionNumber">
        /// The revision number of the changeset.
        /// </param>
        /// <param name="phase">
        /// The phase of the changeset.
        /// </param>
        public ChangesetPhase(int revisionNumber, Phases phase)
        {
            _RevisionNumber = revisionNumber;
            _Phase = phase;
        }

        /// <summary>
        /// Gets the phase of the changeset.
        /// </summary>
        public Phases Phase
        {
            get
            {
                return _Phase;
            }
        }

        /// <summary>
        /// Gets the revision number of the changeset.
        /// </summary>
        public int RevisionNumber
        {
            get
            {
                return _RevisionNumber;
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        public bool Equals(ChangesetPhase other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other._RevisionNumber == _RevisionNumber && Equals(other._Phase, _Phase);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="Object"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Object"/> is equal to the current <see cref="Object"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <param name="obj">
        /// The <see cref="Object"/> to compare with the current <see cref="Object"/>. 
        /// </param><exception cref="NullReferenceException">The <paramref name="obj"/> parameter is <c>null</c>.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(ChangesetPhase))
                return false;
            return Equals((ChangesetPhase)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (_RevisionNumber * 397) ^ _Phase.GetHashCode();
            }
        }
    }
}