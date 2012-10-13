using System;
using System.IO;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class RevertTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("API")]
        public void Revert_NoNames_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Repo.Revert(new RevertCommand()));
        }

        [Test]
        [Category("Integration")]
        public void Revert_NoRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Revert("test1.txt"));
        }

        [Test]
        [Category("Integration")]
        public void Revert_NonExistantFile_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            Assert.Throws<MercurialExecutionException>(() => Repo.Revert("test1.txt"));
        }

        [Test]
        [Category("Integration")]
        public void Revert_ChangedTrackedFile_RevertsChanges()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test1.txt", "123", "dummy", true);
            string filePath = Path.Combine(Repo.Path, "test1.txt");
            File.WriteAllText(filePath, "ABC");

            Assert.That(File.ReadAllText(filePath), Is.EqualTo("ABC"));

            Repo.Revert("test1.txt");

            Assert.That(File.ReadAllText(filePath), Is.EqualTo("123"));
        }
    }
}