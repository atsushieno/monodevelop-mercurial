using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class RollbackTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Rollback_NoRepo_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Rollback());
        }

        [Test]
        [Category("Integration")]
        public void Rollback_OneChangeset_RemovesChangeset()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "test", "test", true);
            Repo.Rollback();

            Changeset[] log = Repo.Log().ToArray();

            Assert.That(log, Is.Empty);
        }
    }
}