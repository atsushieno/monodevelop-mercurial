using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class ManifestTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("API")]
        public void Manifest_NoRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Manifest());
        }

        [Test]
        [Category("Integration")]
        public void Manifest_InitializedRepository_ReturnsEmptyManifest()
        {
            Repo.Init();
            string[] manifest = Repo.Manifest().ToArray();

            Assert.That(manifest.Length, Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public void Manifest_WithFileCommitted_ReturnsFileInManifest()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test1.txt", "dummy", "dummy", true);

            string[] manifest = Repo.Manifest().ToArray();

            CollectionAssert.AreEqual(
                new[]
                {
                    "test1.txt"
                }, manifest);
        }

        [Test]
        [Category("Integration")]
        public void Manifest_ForOlderRevisions_ReturnsCorrectManifest()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test1.txt", "dummy", "dummy", true);
            WriteTextFileAndCommit(Repo, "test2.txt", "dummy", "dummy", true);

            string[] manifest = Repo.Manifest(new ManifestCommand()
                .WithRevision(0)).ToArray();

            CollectionAssert.AreEqual(
                new[]
                {
                    "test1.txt"
                }, manifest);
        }
    }
}