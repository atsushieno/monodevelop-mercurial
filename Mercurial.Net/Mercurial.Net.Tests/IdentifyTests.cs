using System.Linq;
using System.Net;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class IdentifyTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Identify_NoRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Identify());
        }

        [Test]
        [Category("Integration")]
        public void Identify_RepositoryOverHttp_ReturnsRevSpec()
        {
            IdentifyCommand cmd = new IdentifyCommand().WithPath("https://hg01.codeplex.com/mercurialnet");

            NonPersistentClient.Execute(cmd);

            Assert.That(cmd.Result, Is.Not.Null);
        }

        [Test]
        [Category("Integration")]
        public void Identify_WebSiteThatIsntRepository_ThrowsMercurialExecutionException()
        {
            try
            {
                new WebClient().DownloadString("http://localhost");
            }
            catch (WebException)
            {
                Assert.Inconclusive("No web server set up locally, test not executed");
                return;
            }

            IdentifyCommand cmd = new IdentifyCommand().WithPath("http://localhost");

            Assert.Throws<MercurialExecutionException>(() => NonPersistentClient.Execute(cmd));
        }

        [Test]
        [Category("Integration")]
        public void Identify_WebSiteThatDoesNotExist_ThrowsMercurialExecutionException()
        {
            IdentifyCommand cmd = new IdentifyCommand().WithPath("http://localhostxyzklm");

            Assert.Throws<MercurialExecutionException>(() => NonPersistentClient.Execute(cmd));
        }

        [Test]
        [Category("Integration")]
        public void Identify_WithRepository_ReturnsRevSpec()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy", "dummy", true);

            RevSpec hashViaLog = Repo.Log().First().Revision;
            RevSpec hashViaIdentify = Repo.Identify();

            Assert.That(hashViaLog.ToString(), Is.StringStarting(hashViaIdentify.ToString()));
        }
    }
}