using System.IO;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class VerifyTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Verify_CorruptFilelog_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy", "dummy", true);
            string fileName = PathEx.Combine(Repo.Path, ".hg", "store", "data", "test.txt.i");
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                stream.Position = stream.Length - 1;
                int b = stream.ReadByte();
                stream.Position = stream.Length - 1;
                stream.WriteByte((byte)(b ^ 0xff));
            }

            Assert.Throws<MercurialExecutionException>(() => Repo.Verify());
        }

        [Test]
        [Category("Integration")]
        public void Verify_MissingFilelog_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy", "dummy", true);
            File.Delete(PathEx.Combine(Repo.Path, ".hg", "store", "data", "test.txt.i"));

            Assert.Throws<MercurialExecutionException>(() => Repo.Verify());
        }

        [Test]
        [Category("Integration")]
        public void Verify_NoProblems_ReturnsWithoutThrowingException()
        {
            Repo.Init();
            Repo.Verify();
        }

        [Test]
        [Category("Integration")]
        public void Verify_NoRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Verify());
        }

        [Test]
        [Category("Integration")]
        public void Verify_TruncatedFilelog_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy", "dummy", true);
            string fileName = PathEx.Combine(Repo.Path, ".hg", "store", "data", "test.txt.i");
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                stream.SetLength(stream.Length - 1);
            }

            Assert.Throws<MercurialExecutionException>(() => Repo.Verify());
        }
    }
}