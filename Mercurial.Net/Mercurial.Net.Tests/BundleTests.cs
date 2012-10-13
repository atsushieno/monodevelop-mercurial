using System;
using System.IO;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class BundleTests : DualRepositoryTestsBase
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \r\n\t")]
        [Test]
        [Category("API")]
        public void Bundle_NullOrEmptyFileName_ThrowsArgumentNullException(string input)
        {
            Assert.Throws<ArgumentNullException>(() => Repo1.Bundle(input, "http://dummy/repo"));
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \r\n\t")]
        [Test]
        [Category("API")]
        public void Bundle_NullOrEmptyDestination_ThrowsArgumentNullException(string input)
        {
            string tempFileName = Path.GetTempFileName();

            Assert.Throws<ArgumentNullException>(() => Repo1.Bundle(tempFileName, input));
        }

        [Test]
        [Category("Integration")]
        public void Bundle_DestinationIsACompleteClone_ThrowsNoChangesFoundMercurialExecutionExceptionAndLeavesFileAsIs()
        {
            string bundleFileName = GetTempFileName();

            Repo1.Init();
            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy", "dummy", true);

            Repo2.Clone(Repo1.Path);

            Assert.Throws<NoChangesFoundMercurialExecutionException>(() => Repo1.Bundle(bundleFileName, Repo2.Path));

            long length;
            using (var stream = new FileStream(bundleFileName, FileMode.Open))
            {
                length = stream.Length;
            }
            Assert.That(length, Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public void Bundle_DestinationIsEmpty_ProducesBundleFile()
        {
            string bundleFileName = GetTempFileName();

            Repo1.Init();
            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy", "dummy", true);

            Repo2.Init();

            Repo1.Bundle(bundleFileName, Repo2.Path);

            long length;
            using (var stream = new FileStream(bundleFileName, FileMode.Open))
            {
                length = stream.Length;
            }
            Assert.That(length, Is.GreaterThan(0));
        }

        [Test]
        [Category("Integration")]
        public void Bundle_NoRepository_ThrowsMercurialExecutionException()
        {
            string tempFileName = GetTempFileName();

            Assert.Throws<MercurialExecutionException>(() => Repo1.Bundle(tempFileName));
        }

        [Test]
        [Category("Integration")]
        public void Bundle_PulledIntoEmptyRepository_ProducesCloneOfSource()
        {
            string bundleFileName = GetTempFileName();

            Repo1.Init();
            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy", "dummy", true);

            Repo2.Init();

            Repo1.Bundle(bundleFileName, Repo2.Path);
            Repo2.Pull(bundleFileName);

            CollectionAssert.AreEqual(Repo1.Log(new LogCommand().WithIncludePathActions()), Repo2.Log(new LogCommand().WithIncludePathActions()));
        }

        [Test]
        [Category("Integration")]
        public void Bundle_UnbundledIntoEmptyRepository_ProducesCloneOfSource()
        {
            string bundleFileName = GetTempFileName();

            Repo1.Init();
            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy", "dummy", true);

            Repo2.Init();

            Repo1.Bundle(bundleFileName, Repo2.Path);
            Repo2.Unbundle(bundleFileName);

            CollectionAssert.AreEqual(Repo1.Log(new LogCommand().WithIncludePathActions()), Repo2.Log(new LogCommand().WithIncludePathActions()));
        }

        [Test]
        [Category("Integration")]
        public void Bundle_UnrelatedDestinationRepository_ThrowsMercurialExecutionException()
        {
            Repo1.Init();
            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy", "dummy", true);

            Repo2.Init();
            WriteTextFileAndCommit(Repo2, "test2.txt", "dummy", "dummy", true);

            Assert.Throws<MercurialExecutionException>(() => Repo1.Bundle(GetTempFileName(), Repo2.Path));
        }

        [Test]
        [Category("API")]
        public void Bundle_WithJustCommandButMissingFileName_ThrowsInvalidOperationException()
        {
            var command = new BundleCommand
            {
                Destination = "http://dummy/repo",
            };
            Assert.Throws<InvalidOperationException>(() => Repo1.Bundle(command));
        }
    }
}