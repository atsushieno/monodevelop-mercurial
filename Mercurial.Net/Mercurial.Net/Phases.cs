namespace Mercurial
{
    /// <summary>
    /// This enum is used by the <see cref="PhaseCommand"/> to specify the phase for a changeset.
    /// </summary>
    public enum Phases
    {
        /// <summary>
        /// This is not a phase, instead it instructs the <see cref="PhaseCommand"/> to simply list the
        /// phases of the matching changesets.
        /// </summary>
        List,

        /// <summary>
        /// This phase is used to hide changesets from outgoing, push, and pull commands, as though
        /// they were not present in the repository.
        /// </summary>
        Secret,

        /// <summary>
        /// A draft changeset is a new changeset in the local repository that has not yet been
        /// published.
        /// </summary>
        Draft,

        /// <summary>
        /// A public changeset is a changeset that is considered published, typically through a push
        /// command.
        /// </summary>
        Public
    }
}
