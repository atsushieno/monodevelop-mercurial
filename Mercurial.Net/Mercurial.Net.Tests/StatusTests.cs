using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class StatusTests : SingleRepositoryTestsBase
    {
        private string AddAndCommitFile()
        {
            string filePath = Path.Combine(Repo.Path, "test1.txt");
            File.WriteAllText(filePath, "dummy content");
            Repo.Commit(
                "dummy", new CommitCommand
                {
                    AddRemove = true
                });
            return filePath;
        }

        [Test]
        [Category("Integration")]
        public void StatusIncludingAll_AfterCommittingAFile_ShowsFileAsClean()
        {
            Repo.Init();
            AddAndCommitFile();
            FileStatus[] status = Repo.Status(
                new StatusCommand
                {
                    Include = FileStatusIncludes.All,
                }).ToArray();
            CollectionAssert.AreEqual(
                status, new[]
                {
                    new FileStatus(FileState.Clean, "test1.txt")
                });
        }

        [Test]
        [Category("Integration")]
        public void Status_AfterCommittingAFile_DoesNotListFile()
        {
            Repo.Init();
            AddAndCommitFile();
            FileStatus[] status = Repo.Status().ToArray();
            CollectionAssert.IsEmpty(status);
        }

        [Test]
        [Category("Integration")]
        public void Status_AfterDeletingAFile_ShowsFileAsMissing()
        {
            Repo.Init();
            File.Delete(AddAndCommitFile());
            FileStatus[] status = Repo.Status().ToArray();
            CollectionAssert.AreEqual(
                status, new[]
                {
                    new FileStatus(FileState.Missing, "test1.txt")
                });
        }

        [Test]
        [Category("Integration")]
        public void Status_NewFile_ShowsFileAsUnknown()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy content");
            FileStatus[] status = Repo.Status().ToArray();
            CollectionAssert.AreEqual(
                status, new[]
                {
                    new FileStatus(FileState.Unknown, "test1.txt")
                });
        }

        [Test]
        [Category("Integration")]
        public void Status_OnInitializedRepository_ReturnsEmptyStatusCollection()
        {
            Repo.Init();
            Repo.Status().ToArray();
        }

        [Test]
        [Category("Integration")]
        public void Status_OnUninitializedRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Status().ToArray());
        }
    }
}