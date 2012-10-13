using System;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class BranchTests : SingleRepositoryTestsBase
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \t\n\r")]
        [Test]
        [Category("API")]
        public void Branch_WithNullOrEmptyName_ThrowsArgumentNullException(string input)
        {
            Assert.Throws<ArgumentNullException>(() => Repo.Branch(input));
        }

        [Test]
        [Category("Integration")]
        public void BranchAndCommit_CreatesNewBranch()
        {
            Repo.Init();
            Repo.Branch("newbranch");
            Repo.Commit("dummy");
            Assert.That(Repo.Branch(), Is.EqualTo("newbranch"));
        }

        [Test]
        [Category("Integration")]
        public void Branch_WithoutParameters_ReturnsCurrentBranch()
        {
            Repo.Init();
            string currentBranch = Repo.Branch();

            Assert.That(currentBranch, Is.EqualTo("default"));
        }

        [Test]
        [Category("Integration")]
        public void Branch_WithoutRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Branch("newbranch"));
        }
    }
}