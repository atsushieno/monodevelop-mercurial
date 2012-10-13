using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class UnicodeTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Commit_WithUnicodeCharactersInCommitMessage_ProducesLogMessageWithTheSameText()
        {
            const string commitMessage = "Unicode:æøåÆØÅí";

            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy", commitMessage, true);
            string logMessage = Repo.Log().First().CommitMessage;

            Assert.That(logMessage, Is.EqualTo(commitMessage));
        }

        [Test]
        [Category("Integration")]
        public void Commit_WithUnicodeCharactersInFileName_ProducesLogMessageWithTheSameFileName()
        {
            const string fileName = "æøåÆØÅí.txt";

            Repo.Init();
            WriteTextFileAndCommit(Repo, fileName, "dummy", "dummy", true);
            string logFileName = Repo.Log(
                new LogCommand
                {
                    IncludePathActions = true,
                }).First().PathActions.First().Path;

            Assert.That(logFileName, Is.EqualTo(logFileName));
        }
    }
}