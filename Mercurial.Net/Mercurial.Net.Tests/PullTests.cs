using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class PullTests : DualRepositoryTestsBase
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
        public void Pull_WithNullOrEmptySource_ThrowsMercurialExecutionException(string source)
        {
            Assert.Throws<ArgumentNullException>(() => Repo2.Pull(source));
        }

        [TestCase(false)]
        [TestCase(true)]
        [Test]
        [Category("Integration")]
        public void Pull_WithUpdateOption_UpdatesAccordingly(bool doUpdate)
        {
            WriteTextFileAndCommit(Repo1, "test1.txt", "initial contents", "initial commit", true);
            Repo2.Pull(
                Repo1.Path, new PullCommand
                {
                    Update = doUpdate,
                });

            Assert.That(File.Exists(Path.Combine(Repo2.Path, "test1.txt")), Is.EqualTo(doUpdate));
        }

        [Test]
        [Category("Integration")]
        public void Pull_Branch_OnlyPullsThatBranch()
        {
            WriteTextFileAndCommit(Repo1, "test1.txt", "initial contents", "initial commit", true);
            Repo2.Pull(Repo1.Path);

            Repo1.Branch("newbranch");
            WriteTextFileAndCommit(Repo1, "test2.txt", "initial contents", "2nd commit", true);
            Repo1.Update(0);
            WriteTextFileAndCommit(Repo1, "test3.txt", "initial contents", "3rd commit", true);

            Changeset[] log = Repo2.Log().ToArray();
            Assert.That(log.Length, Is.EqualTo(1));

            Repo2.Pull(
                Repo1.Path, new PullCommand
                {
                    Branches =
                        {
                            "newbranch",
                        },
                });
            log = Repo2.Log().ToArray();
            Assert.That(log.Length, Is.EqualTo(2));
        }

        [Test]
        [Category("Integration")]
        public void Pull_ForceIntoUnrelatedNonEmptyRepository_PerformsPull()
        {
            WriteTextFileAndCommit(Repo1, "test1.txt", "initial contents", "initial commit", true);
            WriteTextFileAndCommit(Repo2, "test2.txt", "initial contents", "initial commit", true);

            Repo2.Pull(
                Repo1.Path, new PullCommand
                {
                    Force = true,
                });

            Assert.That(Repo2.Log().Count(), Is.EqualTo(2));
        }

        [Test]
        [Category("Integration")]
        public void Pull_FromClone_GetsNewChangesets()
        {
            Repository clone = GetRepository();
            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy contents", "dummy", true);

            clone.Clone(Repo1.Path);

            Assert.That(clone.Log().Count(), Is.EqualTo(1));

            WriteTextFileAndCommit(Repo1, "test2.txt", "dummy contents", "dummy", true);
            clone.Pull();

            Assert.That(clone.Log().Count(), Is.EqualTo(2));
        }

        [Test]
        [Category("Integration")]
        public void Pull_FromOtherWithRevisionSpecification_OnlyPullsInRelevantChanges()
        {
            WriteTextFileAndCommit(Repo1, "test1.txt", "initial contents", "initial commit", true);
            WriteTextFileAndCommit(Repo1, "test1.txt", "changed contents", "2nd commit", false);
            WriteTextFileAndCommit(Repo1, "test1.txt", "changed contents again", "3rd commit", false);

            Repo2.Pull(
                Repo1.Path, new PullCommand
                {
                    Revisions =
                        {
                            RevSpec.Single(1),
                        },
                });

            Changeset[] pulledChangesets = Repo2.Log().OrderBy(c => c.RevisionNumber).ToArray();
            Changeset[] originalChangesets = Repo1.Log(
                new LogCommand
                {
                    Revisions =
                        {
                            RevSpec.Range(0, 1),
                        },
                }).OrderBy(c => c.RevisionNumber).ToArray();
            CollectionAssert.AreEqual(pulledChangesets, originalChangesets);
        }

        [Test]
        [Category("Integration")]
        public void Pull_FromOther_PullsInAllChanges()
        {
            WriteTextFileAndCommit(Repo1, "test1.txt", "initial contents", "initial commit", true);
            WriteTextFileAndCommit(Repo1, "test1.txt", "changed contents", "2nd commit", false);

            Repo2.Pull(Repo1.Path);

            CollectionAssert.AreEqual(Repo2.Log(), Repo1.Log());
        }

        [Test]
        [Category("Integration")]
        public void Pull_IntoUnrelatedNonEmptyRepository_ThrowsMercurialExecutionException()
        {
            WriteTextFileAndCommit(Repo1, "test1.txt", "initial contents", "initial commit", true);
            WriteTextFileAndCommit(Repo2, "test2.txt", "initial contents", "initial commit", true);

            Assert.Throws<MercurialExecutionException>(() => Repo2.Pull(Repo1.Path));
        }

        [Test]
        [Category("Integration")]
        public void Pull_WithoutSource_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo2.Pull());
        }
    }
}