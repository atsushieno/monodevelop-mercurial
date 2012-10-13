using System;
using System.IO;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class RepositoryTests
    {
        [Test]
        [Category("API")]
        public void Constructor_WithEmptyRootPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Repository(string.Empty));
        }

        [Test]
        [Category("API")]
        public void Constructor_WithExistingPath_ProducesInstanceWithCorrectRootPathProperty()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            try
            {
                using (var repo = new Repository(tempPath, new NonPersistentClientFactory()))
                {
                    Assert.That(repo.Path, Is.EqualTo(tempPath));
                }
            }
            finally
            {
                Directory.Delete(tempPath);
            }
        }

        [Test]
        [Category("API")]
        public void Constructor_WithNullRootPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Repository(null, new NonPersistentClientFactory()));
        }

        [Test]
        [Category("API")]
        public void Constructor_WithPathToNonExistentDirectory_ThrowsDirectoryNotFoundException()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Assert.Throws<DirectoryNotFoundException>(() => new Repository(tempPath, new NonPersistentClientFactory()));
        }

        [Test]
        [Category("API")]
        public void Constructor_WithWhitespaceRootPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Repository("  ", new NonPersistentClientFactory()));
        }
    }
}