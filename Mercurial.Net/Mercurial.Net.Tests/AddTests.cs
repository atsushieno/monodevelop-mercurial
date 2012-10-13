using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class AddTests : SingleRepositoryTestsBase
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \t\n\r")]
        [Test]
        [Category("API")]
        public void Add_NullOrEmptyPath_ThrowsArgumentNullException(string input)
        {
            Assert.Throws<ArgumentNullException>(() => Repo.Add(input));
        }

        [Test]
        [Category("Integration")]
        public void Add_ExistingFile_AddsItToTheRepository()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test.txt"), "contents");
            Repo.Add("test.txt");
            FileStatus[] status = Repo.Status().ToArray();

            CollectionAssert.AreEqual(
                status, new[]
                {
                    new FileStatus(FileState.Added, "test.txt"),
                });
        }

        [Test]
        [Category("Integration")]
        public void Add_ExistingFileWithUnicodeCharacterInName_AddsItToTheRepository()
        {
            Repo.Init();
            const string filename = "testæøåÆØÅ.txt";
            File.WriteAllText(Path.Combine(Repo.Path, filename), "contents");
            Repo.Add(filename);
            FileStatus[] status = Repo.Status().ToArray();

            CollectionAssert.AreEqual(
                status, new[]
                {
                    new FileStatus(FileState.Added, filename),
                });
        }

        [Test]
        [Category("Integration")]
        public void Add_FileDoesNotExist_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            Assert.Throws<MercurialExecutionException>(() => Repo.Add("test.txt"));
        }

        [Test]
        [Category("Integration")]
        public void Add_InUninitializedRepository_ThrowsMercurialExecutionException()
        {
            File.WriteAllText(Path.Combine(Repo.Path, "test.txt"), "contents");
            Assert.Throws<MercurialExecutionException>(() => Repo.Add("test.txt"));
        }

        [Test]
        [Category("API")]
        public void Validate_NoPaths_ThrowsInvalidOperationException()
        {
            var command = new AddCommand();

            Assert.Throws<InvalidOperationException>(command.Validate);
        }

        [Test]
        [Category("API")]
        public void Validate_WithPath_ReturnsWithoutThrowingException()
        {
            var command = new AddCommand();
            command.Paths.Add("test.txt");

            command.Validate();
        }
    }
}