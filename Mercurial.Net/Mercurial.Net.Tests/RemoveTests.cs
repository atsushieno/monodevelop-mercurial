using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class RemoveTests : SingleRepositoryTestsBase
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \t\n\r")]
        [Test]
        [Category("API")]
        public void Remove_NullOrEmptyPath_ThrowsArgumentNullException(string input)
        {
            Assert.Throws<ArgumentNullException>(() => Repo.Remove(input));
        }

        [Test]
        [Category("Integration")]
        public void Remove_UncommittedFileWithForce_RemovesFile()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test.txt"), "contents");
            Repo.Add("test.txt");
            Repo.Remove(
                "test.txt", new RemoveCommand
                {
                    ForceRemoval = true,
                });
            CollectionAssert.AreEqual(
                Repo.Status(),
                new[]
                {
                    new FileStatus(FileState.Unknown, "test.txt"),
                });
        }

        [Test]
        [Category("Integration")]
        public void Remove_DeletedFileWithRecordDeletesOption_RemovesFile()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy", "dummy", true);
            File.Delete(Path.Combine(Repo.Path, "test.txt"));
            Repo.Remove(
                new RemoveCommand
                {
                    RecordDeletes = true,
                });
            FileStatus[] status = Repo.Status().ToArray();

            CollectionAssert.AreEqual(
                status, new[]
                {
                    new FileStatus(FileState.Removed, "test.txt"),
                });
        }

        [Test]
        [Category("Integration")]
        public void Remove_FileDoesNotExist_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            Assert.Throws<MercurialExecutionException>(() => Repo.Remove("test.txt"));
        }

        [Test]
        [Category("Integration")]
        public void Remove_InUninitializedRepository_ThrowsMercurialExecutionException()
        {
            File.WriteAllText(Path.Combine(Repo.Path, "test.txt"), "contents");
            Assert.Throws<MercurialExecutionException>(() => Repo.Remove("test.txt"));
        }

        [Test]
        [Category("Integration")]
        public void Remove_UncommittedFile_ThrowsMercurialExecutionException()
        {
            if (ClientExecutable.GetVersion() < new Version(1, 7, 0, 0))
                Assert.Ignore("This is not reported as a problem in Mercurial <1.7");
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test.txt"), "contents");
            Repo.Add("test.txt");
            Assert.Throws<MercurialExecutionException>(() => Repo.Remove("test.txt"));
        }
    }
}