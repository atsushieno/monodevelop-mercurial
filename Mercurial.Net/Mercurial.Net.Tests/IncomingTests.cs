using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class IncomingTests : DualRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Incoming_CloneOfMasterWithExtraChanges_ReturnsTheExtraChangesets()
        {
            const string commitMessage = "commit message in clone";

            Repo1.Init();
            Repo2.Clone(Repo1.Path);
            WriteTextFileAndCommit(Repo1, "test.txt", "dummy", commitMessage, true);
            Changeset[] incoming = Repo2.Incoming().ToArray();

            Assert.That(incoming.Length, Is.EqualTo(1));
            Assert.That(incoming[0].CommitMessage, Is.EqualTo(commitMessage));
        }

        [Test]
        [Category("Integration")]
        public void Incoming_CloneOfMasterWithNoExtraChanges_ReturnsEmptyCollection()
        {
            Repo1.Init();
            Repo2.Clone(Repo1.Path);
            Changeset[] incoming = Repo2.Incoming().ToArray();

            Assert.That(incoming.Length, Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public void Incoming_RepositoryWithNoDefaultDestination_ThrowsMercurialExecutionException()
        {
            Repo2.Init();
            Assert.Throws<MercurialExecutionException>(() => Repo2.Incoming());
        }

        [Test]
        [Category("Integration")]
        public void Incoming_SavingTheBundle_AllowsPullFromBundle()
        {
            string bundleFileName = Path.GetTempFileName();

            Repo1.Init();
            Repo2.Clone(Repo1.Path);

            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy", "dummy", true);
            Repo2.Incoming(new IncomingCommand().WithBundleFileName(bundleFileName));

            Changeset[] log = Repo2.Log().ToArray();
            Assert.That(log.Length, Is.EqualTo(0));

            Repo2.Pull(bundleFileName);

            log = Repo2.Log().ToArray();
            Assert.That(log.Length, Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public void Incoming_UninitializedRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo2.Incoming());
        }

        [Test]
        [Category("Integration")]
        public void Incoming_UnrelatedRepositoryWithForce_ReturnsCollectionOfChangesets()
        {
            const string commitMessage = "commit message in clone";

            Repo1.Init();
            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy", commitMessage, true);

            Repo2.Init();
            WriteTextFileAndCommit(Repo2, "test2.txt", "dummy", "dummy", true);
            Changeset[] incoming = Repo2.Incoming(Repo1.Path, new IncomingCommand().WithForce()).ToArray();

            Assert.That(incoming.Length, Is.EqualTo(1));
            Assert.That(incoming[0].CommitMessage, Is.EqualTo(commitMessage));
        }

        [Test]
        [Category("Integration")]
        public void Incoming_UnrelatedRepositoryWithoutForce_ThrowsMercurialExecutionException()
        {
            Repo1.Init();
            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy", "dummy", true);

            Repo2.Init();
            WriteTextFileAndCommit(Repo2, "test2.txt", "dummy", "dummy", true);

            Assert.Throws<MercurialExecutionException>(() => Repo2.Incoming(Repo1.Path));
        }
    }
}