using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class CatTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Cat_NoRepo_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Cat("dummy.txt"));
        }

        [Test]
        [Category("Integration")]
        public void Cat_MissingFile_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            Assert.Throws<MercurialExecutionException>(() => Repo.Cat("dummy.txt"));
        }

        [Test]
        [Category("Integration")]
        public void Cat_ExistingFile_ReturnsContentsOfThatFile()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "test contents", "dummy", true);

            string contents = Repo.Cat("test.txt");

            Assert.That(contents, Is.EqualTo("test contents"));
        }
    }
}