using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class AddRemoveTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void AddRemove_WithoutOptions_AddsAllNewFiles()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "test1.txt contents");
            File.WriteAllText(Path.Combine(Repo.Path, "test2.txt"), "test2.txt contents");

            Repo.AddRemove();
            IEnumerable<FileStatus> status = Repo.Status();

            CollectionAssert.AreEqual(
                status, new[]
                {
                    new FileStatus(FileState.Added, "test1.txt"), new FileStatus(FileState.Added, "test2.txt"),
                });
        }

        [Test]
        [Category("Integration")]
        public void AddRemove_WithoutOptions_RemovesAllDeletedFiles()
        {
            string file1Path = Path.Combine(Repo.Path, "test1.txt");
            string file2Path = Path.Combine(Repo.Path, "test2.txt");

            Repo.Init();
            File.WriteAllText(file1Path, "test1.txt contents");
            File.WriteAllText(file2Path, "test2.txt contents");
            Repo.AddRemove();
            Repo.Commit("dummy");
            File.Delete(file1Path);
            File.Delete(file2Path);

            Repo.AddRemove();
            IEnumerable<FileStatus> status = Repo.Status();

            CollectionAssert.AreEqual(
                status, new[]
                {
                    new FileStatus(FileState.Removed, "test1.txt"), new FileStatus(FileState.Removed, "test2.txt"),
                });
        }

        [Test]
        [Category("API")]
        public void Similarity_Above100_ThrowsArgumentOutOfRangeException()
        {
            var command = new AddRemoveCommand();

            Assert.Throws<ArgumentOutOfRangeException>(() => command.Similarity = 101);
        }

        [Test]
        [Category("API")]
        public void Similarity_BelowZero_ThrowsArgumentOutOfRangeException()
        {
            var command = new AddRemoveCommand();

            Assert.Throws<ArgumentOutOfRangeException>(() => command.Similarity = -1);
        }

        [Test]
        [Category("API")]
        public void Similarity_Between0And100_SetsProperty()
        {
            var command = new AddRemoveCommand();

            for (int index = 0; index <= 100; index++)
            {
                command.Similarity = index;
                Assert.That(command.Similarity, Is.EqualTo(index));
            }
        }
    }
}