using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class CommandTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Execute_WithInitCommand_InitializesTheRepository()
        {
            Repo.Execute(new InitCommand());
            Repo.Log().ToArray();
        }
    }
}