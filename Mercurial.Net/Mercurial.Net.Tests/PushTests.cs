using System;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class PushTests : DualRepositoryTestsBase
    {
        #region Setup/Teardown

        public override void SetUp()
        {
            base.SetUp();
            Repo1.Init();
            Repo2.Init();
        }

        #endregion

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \n\r\t")]
        [Test]
        [Category("API")]
        public void Push_WithNullOrEmptyDestination_ThrowsMercurialExecutionException(string destination)
        {
            Assert.Throws<ArgumentNullException>(() => Repo2.Push(destination));
        }

        [Test]
        [Category("Integration")]
        public void Push_IntoUnrelatedRepositoryWithForce_PushesSuccessfullyAndCreatesAnotherHead()
        {
            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy", "dummy", true);
            WriteTextFileAndCommit(Repo2, "test2.txt", "dummy", "dummy", true);

            Repo2.Push(
                Repo1.Path, new PushCommand
                {
                    Force = true,
                });

            Changeset[] log = Repo1.Heads().ToArray();

            Assert.That(log.Length, Is.EqualTo(2));
        }

        [Test]
        [Category("Integration")]
        public void Push_IntoUnrelatedRepository_ThrowsMercurialExecutionException()
        {
            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy", "dummy", true);
            WriteTextFileAndCommit(Repo2, "test2.txt", "dummy", "dummy", true);

            Assert.Throws<MercurialExecutionException>(() => Repo2.Push(Repo1.Path));
        }
    }
}