using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class CommitTests : SingleRepositoryTestsBase
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \t \n \r")]
        [Test]
        [Category("Integration")]
        public void CommitWithMessage_NullOrEmptyMessage_ThrowsArgumentNullException(string testCase)
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy content");
            Assert.Throws<ArgumentNullException>(
                () => Repo.Commit(
                    testCase, new CommitCommand
                    {
                        AddRemove = true,
                    }));
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \t \n \r")]
        [Test]
        [Category("Integration")]
        public void CommitWithMessageInOptions_NullOrEmptyMessage_ThrowsArgumentNullException(string testCase)
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy content");
            Assert.Throws<ArgumentNullException>(
                () => Repo.Commit(
                    new CommitCommand
                    {
                        Message = testCase,
                        AddRemove = true,
                    }));
        }

        [Test]
        [Category("Integration")]
        public void CommitWithMessageInOptions_WithChanges_CommitsAChangeset()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy content");
            Repo.Commit(
                new CommitCommand
                {
                    Message = "dummy",
                    AddRemove = true,
                });
            Assert.That(Repo.Log().Count(), Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public void Commit_ListingFile_OnlyCommitsThatFile()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy");
            File.WriteAllText(Path.Combine(Repo.Path, "test2.txt"), "dummy");
            Repo.AddRemove();

            Repo.Commit(new CommitCommand().WithMessage("dummy").WithPath("test1.txt"));

            FileStatus[] status = Repo.Status().ToArray();

            Assert.That(status.Length, Is.EqualTo(1));
            Assert.That(status[0].Path, Is.EqualTo("test2.txt"));
        }

        [Test]
        [Category("Integration")]
        public void Commit_NoChanges_DoesNotAddAChangeset()
        {
            Repo.Init();
            try
            {
                Repo.Commit("dummy");
            }
            catch (MercurialExecutionException)
            {
                // Swallow this one
            }
            CollectionAssert.IsEmpty(Repo.Log());
        }

        [Test]
        [Category("Integration")]
        public void Commit_NoChanges_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            Assert.Throws<MercurialExecutionException>(() => Repo.Commit("dummy"));
        }

        [Test]
        [Category("Integration")]
        public void Commit_NoRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Commit("dummy"));
        }

        [Test]
        [Category("API")]
        public void Commit_NullOptions_ThrowsArgumentNullException()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy content");
            Assert.Throws<ArgumentNullException>(() => Repo.Commit(null));
        }

        [Test]
        [Category("Integration")]
        public void Commit_WithChanges_CommitsAChangeset()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy content");
            Repo.Commit(
                "dummy", new CommitCommand
                {
                    AddRemove = true,
                });
            Assert.That(Repo.Log().Count(), Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public void Commit_WithOneExcludePattern_DoesNotCommitThatFile()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy content");
            File.WriteAllText(Path.Combine(Repo.Path, "test2.txt"), "dummy content");
            Repo.Commit(
                "dummy", new CommitCommand
                {
                    AddRemove = true,
                    OverrideAuthor = "Dummy User <dummy@company.com>",
                    ExcludePatterns =
                        {
                            "test1.txt",
                        },
                });
            Collection<ChangesetPathAction> filesCommitted = Repo.Log(
                new LogCommand
                {
                    IncludePathActions = true,
                }).First().PathActions;

            CollectionAssert.AreEqual(
                filesCommitted, new[]
                {
                    new ChangesetPathAction
                    {
                        Action = ChangesetPathActionType.Add,
                        Path = "test2.txt"
                    },
                });
        }

        [Test]
        [Category("Integration")]
        public void Commit_WithOneIncludePattern_CommitsJustThatFile()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy content");
            File.WriteAllText(Path.Combine(Repo.Path, "test2.txt"), "dummy content");
            Repo.Commit(
                "dummy", new CommitCommand
                {
                    AddRemove = true,
                    OverrideAuthor = "Dummy User <dummy@company.com>",
                    IncludePatterns =
                        {
                            "test1.txt",
                        },
                });
            Collection<ChangesetPathAction> filesCommitted = Repo.Log(
                new LogCommand
                {
                    IncludePathActions = true,
                }).First().PathActions;

            CollectionAssert.AreEqual(
                filesCommitted, new[]
                {
                    new ChangesetPathAction
                    {
                        Action = ChangesetPathActionType.Add,
                        Path = "test1.txt"
                    },
                });
        }

        [Test]
        [Category("Integration")]
        public void Commit_WithOverriddenTimestamp_CommitsWithThatTimestamp()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy content");
            var timestamp = new DateTime(2010, 1, 17, 18, 23, 59);
            Repo.Commit(
                "dummy", new CommitCommand
                {
                    AddRemove = true,
                    OverrideTimestamp = timestamp,
                });
            Assert.That(Repo.Log().First().Timestamp, Is.EqualTo(timestamp));
        }

        [Test]
        [Category("Integration")]
        public void Commit_WithOverriddenUsername_CommitsWithThatUsername()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy content");
            Repo.Commit(
                "dummy", new CommitCommand
                {
                    AddRemove = true,
                    OverrideAuthor = "Dummy User <dummy@company.com>",
                });
            Assert.That(Repo.Log().First().AuthorName, Is.EqualTo("Dummy User"));
            Assert.That(Repo.Log().First().AuthorEmailAddress, Is.EqualTo("dummy@company.com"));
        }
    }
}