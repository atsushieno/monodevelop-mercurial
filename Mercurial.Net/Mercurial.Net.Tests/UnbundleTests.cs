using System;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class UnbundleTests : DualRepositoryTestsBase
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \t\r\n")]
        [Test]
        [Category("API")]
        public void Unbundle_NullOrEmptyFileName_ThrowsArgumentNullException(string input)
        {
            Repo1.Init();
            Assert.Throws<ArgumentNullException>(() => Repo1.Unbundle(input));
        }

        [Test]
        [Category("Integration")]
        public void Unbundle_InvalidFile_ThrowsMercurialExecutionException()
        {
            Repo1.Init();
            Assert.Throws<MercurialExecutionException>(() => Repo1.Unbundle(GetTempFileName()));
        }

        [Test]
        [Category("Integration")]
        public void Unbundle_NoRepository_ThrowsMercurialExecutionException()
        {
            string bundleFileName = GetTempFileName();

            Repo1.Init();
            WriteTextFileAndCommit(Repo1, "test1.txt", "dummy", "dummy", true);
            Repo1.Bundle(bundleFileName, new BundleCommand().WithAll());

            Assert.Throws<MercurialExecutionException>(() => Repo2.Unbundle(bundleFileName));
        }
    }
}