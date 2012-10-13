using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class PathsTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Paths_FromClone_ListsPathToOriginal()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy", "dummy", true);

            Repository clone = GetRepository();
            clone.Clone(Repo.Path);

            RemoteRepositoryPath[] paths = clone.Paths().ToArray();
            Assert.That(paths.Length, Is.EqualTo(1));
            Assert.That(paths[0].Name, Is.EqualTo("default"));
            Assert.That(paths[0].Path.ToUpperInvariant(), Is.EqualTo(Repo.Path.ToUpperInvariant()));
        }

        [Test]
        [Category("Integration")]
        public void Paths_FromFreshRepository_ReturnsEmptyCollection()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy", "dummy", true);

            RemoteRepositoryPath[] paths = Repo.Paths().ToArray();
            Assert.That(paths.Length, Is.EqualTo(0));
        }
    }
}