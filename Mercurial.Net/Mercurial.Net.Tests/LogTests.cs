using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class LogTests : SingleRepositoryTestsBase
    {
        private void CommitAndRenameFile()
        {
            Repo.Init();
            string file1Path = Path.Combine(Repo.Path, "test1.txt");
            string file2Path = Path.Combine(Repo.Path, "test2.txt");
            File.WriteAllText(file1Path, "dummy content");
            Repo.AddRemove();
            Repo.Commit("initial");
            File.Move(file1Path, file2Path);
            Repo.AddRemove();
            Repo.Commit("renamed");
        }

        private void CreateRepositoryWithTwoCommitsOn18thAnd19thOfJanuary()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy content");
            Repo.Commit(
                "dummy", new CommitCommand
                {
                    AddRemove = true,
                    OverrideTimestamp = new DateTime(2010, 1, 18, 18, 0, 0),
                });
            File.WriteAllText(Path.Combine(Repo.Path, "test2.txt"), "dummy content");
            Repo.Commit(
                "dummy", new CommitCommand
                {
                    AddRemove = true,
                    OverrideTimestamp = new DateTime(2010, 1, 19, 18, 0, 0),
                });
        }

        [Test]
        [Category("Integration")]
        public void Log_ForFileAndWithFollowOption_ShowsCompleteLogForFileIncludingBeforeRename()
        {
            if (ClientExecutable.GetVersion() < new Version(1, 7, 0, 0))
                Assert.Ignore("This is not reported accurately in the log in Mercurial <1.7");

            CommitAndRenameFile();

            Changeset[] log = Repo.Log(
                new LogCommand
                {
                    FollowRenamesAndMoves = true,
                    Path = "test2.txt",
                }).ToArray();

            Assert.That(log.Length, Is.EqualTo(2));
        }

        [Test]
        [Category("Integration")]
        public void Log_ForFileAndWithoutFollowOption_OnlyShowsLogForFileFromRenameAndUp()
        {
            CommitAndRenameFile();

            Changeset[] log = Repo.Log(
                new LogCommand
                {
                    Path = "test2.txt",
                }).ToArray();
            Assert.That(log.Length, Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public void Log_WithDateFilter_OnlyRetrievesLogForThatDate()
        {
            CreateRepositoryWithTwoCommitsOn18thAnd19thOfJanuary();

            Changeset[] log = Repo.Log(
                new LogCommand
                {
                    Date = new DateTime(2010, 1, 18),
                }).ToArray();
            Assert.That(log.Length, Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public void Log_WithExcludePattern_DoesNotChangesetsThatIncludesThatFile()
        {
            Repo.Init();
            foreach (string filename in new[]
            {
                "test1.txt", "test2.txt"
            })
            {
                File.WriteAllText(Path.Combine(Repo.Path, filename), "dummy content");
                Repo.Commit(
                    filename + " added", new CommitCommand
                    {
                        AddRemove = true,
                    });
            }

            Changeset[] log = Repo.Log(
                new LogCommand
                {
                    IncludePatterns =
                        {
                            "test2.txt",
                        },
                }).ToArray();

            Assert.That(log.Length, Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public void Log_WithIncludePattern_DoesNotChangesetsThatDoesntIncludeThatFile()
        {
            Repo.Init();
            foreach (string filename in new[]
            {
                "test1.txt", "test2.txt"
            })
            {
                File.WriteAllText(Path.Combine(Repo.Path, filename), "dummy content");
                Repo.Commit(
                    filename + " added", new CommitCommand
                    {
                        AddRemove = true,
                    });
            }

            Changeset[] log = Repo.Log(
                new LogCommand
                {
                    IncludePatterns =
                        {
                            "test1.txt",
                        },
                }).ToArray();

            Assert.That(log.Length, Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public void Log_WithIncludePatterns_DoesNotChangesetsThatDoesntIncludeThoseFiles()
        {
            Repo.Init();
            foreach (string filename in new[]
            {
                "test1.txt", "test2.txt", "test3.txt"
            })
            {
                File.WriteAllText(Path.Combine(Repo.Path, filename), "dummy content");
                Repo.Commit(
                    filename + " added", new CommitCommand
                    {
                        AddRemove = true,
                    });
            }

            Changeset[] log = Repo.Log(
                new LogCommand
                {
                    IncludePatterns =
                        {
                            "test1.txt",
                            "test3.txt",
                        },
                }).ToArray();

            Assert.That(log.Length, Is.EqualTo(2));
        }

        [Test]
        [Category("Integration")]
        public void Log_WithInitializedRepo_ProducesEmptyLog()
        {
            Repo.Init();
            IEnumerable<Changeset> log = Repo.Log();
            Assert.That(log.Count(), Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public void Log_WithMultipleRevisionFilters_OnlyIncludeMatchingChangesets()
        {
            Repo.Init();
            foreach (string filename in new[]
            {
                "test1.txt", "test2.txt", "test3.txt"
            })
            {
                File.WriteAllText(Path.Combine(Repo.Path, filename), "dummy content");
                Repo.Commit(
                    filename + " added", new CommitCommand
                    {
                        AddRemove = true,
                    });
            }

            Changeset[] log = Repo.Log(
                new LogCommand
                {
                    Revisions =
                        {
                            RevSpec.Range(RevSpec.Single(0), RevSpec.Single(0)),
                            RevSpec.Range(RevSpec.Single(2), RevSpec.Single(2)),
                        },
                }).ToArray();

            Assert.That(log.Length, Is.EqualTo(2));
        }

        [Test]
        [Category("Integration")]
        public void Log_WithSingleCommit_ReturnsLogThatReferencesFileThatWasCommitted()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy content");
            Repo.Commit(
                "dummy", new CommitCommand
                {
                    AddRemove = true,
                });
            IEnumerable<Changeset> log = Repo.Log(
                new LogCommand
                {
                    IncludePathActions = true,
                });
            CollectionAssert.AreEqual(
                log.First().PathActions, new[]
                {
                    new ChangesetPathAction
                    {
                        Action = ChangesetPathActionType.Add,
                        Path = "test1.txt"
                    }
                });
        }

        [Test]
        [Category("Integration")]
        public void Log_WithoutDateFilter_RetrievesWholeLog()
        {
            CreateRepositoryWithTwoCommitsOn18thAnd19thOfJanuary();

            Changeset[] log = Repo.Log().ToArray();
            Assert.That(log.Length, Is.EqualTo(2));
        }

        [Test]
        [Category("Integration")]
        public void Log_WithoutInitializedRepo_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Log());
        }
    }
}