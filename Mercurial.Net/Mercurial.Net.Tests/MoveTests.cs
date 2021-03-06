using System.IO;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class MoveTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("API")]
        public void Move_NoRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Move("test1.txt", "test2.txt"));
        }

        [Test]
        [Category("Integration")]
        public void Move_FilesDoesNotExist_ThrowsMercurialExecutionException()
        {
            Repo.Init();

            Assert.Throws<MercurialExecutionException>(() => Repo.Move("test1.txt", "test2.txt"));
        }

        [Test]
        [Category("Integration")]
        public void Move_TrackedFile_MovesFileOnDisk()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test1.txt", "dummy", "dummy", true);

            Repo.Move("test1.txt", "test2.txt");

            Assert.That(File.Exists(Path.Combine(Repo.Path, "test1.txt")), Is.False);
            Assert.That(File.Exists(Path.Combine(Repo.Path, "test2.txt")), Is.True);
        }

        [Test]
        [Category("Integration")]
        public void Move_IsTracked()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test1.txt", "dummy", "dummy", true);

            Repo.Move("test1.txt", "test2.txt");
            Repo.Commit("dummy");

            Repo.Update(0);

            Assert.That(File.Exists(Path.Combine(Repo.Path, "test1.txt")), Is.True);
            Assert.That(File.Exists(Path.Combine(Repo.Path, "test2.txt")), Is.False);

            Repo.Update(1);

            Assert.That(File.Exists(Path.Combine(Repo.Path, "test1.txt")), Is.False);
            Assert.That(File.Exists(Path.Combine(Repo.Path, "test2.txt")), Is.True);
        }
    }
}