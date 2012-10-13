using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class ForgetTests : SingleRepositoryTestsBase
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \t\n\r")]
        [Test]
        [Category("API")]
        public void Forget_NullOrEmptyPath_ThrowsArgumentNullException(string input)
        {
            Assert.Throws<ArgumentNullException>(() => Repo.Forget(input));
        }

        [Test]
        [Category("Integration")]
        public void Forget_InUninitializedRepository_ThrowsMercurialExecutionException()
        {
            File.WriteAllText(Path.Combine(Repo.Path, "test.txt"), "contents");
            Assert.Throws<MercurialExecutionException>(() => Repo.Forget("test.txt"));
        }

        [Test]
        [Category("Integration")]
        public void Forget_TrackedFile_MarksFileForForgettal()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy", "dummy", true);
            Repo.Forget("test.txt");

            FileStatus[] status = Repo.Status().ToArray();

            CollectionAssert.AreEqual(
                status, new[]
                {
                    new FileStatus(FileState.Removed, "test.txt"),
                });
        }

        [Test]
        [Category("Integration")]
        public void Forget_UnTrackedFile_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test.txt"), "contents");
            Assert.Throws<MercurialExecutionException>(() => Repo.Forget("test.txt"));
        }
    }
}