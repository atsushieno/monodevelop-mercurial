using System;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class CloneTests : SingleRepositoryTestsBase
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \r\n\t")]
        [Test]
        [Category("API")]
        public void Clone_NullOrEmptySource_ThrowsArgumentNullException(string input)
        {
            Assert.Throws<ArgumentNullException>(() => Repo.Clone(input));
        }

        [Test]
        [Category("Integration")]
        public void Clone_ProducesCloneWithSameLog()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test1.txt", "dummy content", "dummy", true);

            Repository cloneRepo = GetRepository();

            cloneRepo.Clone(Repo.Path);
            CollectionAssert.AreEqual(cloneRepo.Log(), Repo.Log());
        }

        [Test]
        [Category("Integration")]
        public void Clone_UpdateToNullRevision_ProducesCloneWithoutFilesInWorkingFolder()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test1.txt", "dummy content", "dummy", true);

            Repository cloneRepo = GetRepository();
            cloneRepo.Clone(
                Repo.Path, new CloneCommand
                {
                    UpdateToRevision = RevSpec.Null,
                });
            CollectionAssert.AreEqual(cloneRepo.Log(), Repo.Log());
            CollectionAssert.AreEqual(
                cloneRepo.Status(
                    new StatusCommand
                    {
                        Include = FileStatusIncludes.All
                    }), new FileStatus[0]);
        }

        [Test]
        [Category("Integration")]
        public void GetHashCodeOfChangeset_FromClone_ProducesSameHashCodeAsInOriginalRepository()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test1.txt", "dummy content", "dummy", true);

            Repository cloneRepo = GetRepository();
            cloneRepo.Clone(
                Repo.Path, new CloneCommand
                {
                    UpdateToRevision = RevSpec.Null,
                });
            Assert.That(cloneRepo.Log().First().GetHashCode(), Is.EqualTo(Repo.Log().First().GetHashCode()));
        }
    }
}