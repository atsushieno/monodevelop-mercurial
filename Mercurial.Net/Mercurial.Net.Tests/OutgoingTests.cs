using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class OutgoingTests : DualRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Outgoing_CloneOfMasterWithExtraChanges_ReturnsTheExtraChangesets()
        {
            const string commitMessage = "commit message in clone";

            Repo1.Init();
            Repo2.Clone(Repo1.Path);
            WriteTextFileAndCommit(Repo2, "test.txt", "dummy", commitMessage, true);
            Changeset[] outgoing = Repo2.Outgoing().ToArray();

            Assert.That(outgoing.Length, Is.EqualTo(1));
            Assert.That(outgoing[0].CommitMessage, Is.EqualTo(commitMessage));
        }

        [Test]
        [Category("Integration")]
        public void Outgoing_CloneOfMasterWithNoExtraChanges_ReturnsEmptyCollection()
        {
            Repo1.Init();
            Repo2.Clone(Repo1.Path);
            Changeset[] outgoing = Repo2.Outgoing().ToArray();

            Assert.That(outgoing.Length, Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public void Outgoing_RepositoryWithNoDefaultDestination_ThrowsMercurialExecutionException()
        {
            Repo2.Init();
            Assert.Throws<MercurialExecutionException>(() => Repo2.Outgoing());
        }

        [Test]
        [Category("Integration")]
        public void Outgoing_UninitializedRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo2.Outgoing());
        }

        [Test]
        [Category("Integration")]
        public void Outgoing_UnrelatedRepositoryWithForce_ReturnsCollectionOfChangesets()
        {
            const string commitMessage = "commit message in clone";

            Repo1.Init();
            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy", "dummy", true);

            Repo2.Init();
            WriteTextFileAndCommit(Repo2, "test2.txt", "dummy", commitMessage, true);
            Changeset[] outgoing = Repo2.Outgoing(Repo1.Path, new OutgoingCommand().WithForce()).ToArray();

            Assert.That(outgoing.Length, Is.EqualTo(1));
            Assert.That(outgoing[0].CommitMessage, Is.EqualTo(commitMessage));
        }

        [Test]
        [Category("Integration")]
        public void Outgoing_UnrelatedRepositoryWithoutForce_ThrowsMercurialExecutionException()
        {
            Repo1.Init();
            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy", "dummy", true);

            Repo2.Init();
            WriteTextFileAndCommit(Repo2, "test2.txt", "dummy", "dummy", true);

            Assert.Throws<MercurialExecutionException>(() => Repo2.Outgoing(Repo1.Path));
        }
    }
}