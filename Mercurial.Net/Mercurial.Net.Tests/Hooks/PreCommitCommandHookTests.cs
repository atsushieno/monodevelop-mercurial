using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests.Hooks
{
    [TestFixture]
    [Category("Integration")]
    public class PreCommitCommandHookTests : SingleRepositoryTestsBase
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "dummy.txt"), "dummy");
            Repo.Add("dummy.txt");
        }

        [Test]
        public void Commit_HookThatPasses_AllowsCommit()
        {
            if (IsUsingPersistentClient)
            {
                Assert.Inconclusive("Hook tests does not function correctly under the persistent client");
                return;
            }

            Repo.SetHook("pre-commit", "ok");

            var command = new CustomCommand("commit")
                .WithAdditionalArgument("-m")
                .WithAdditionalArgument("dummy");
            Repo.Execute(command);

            Assert.That(command.RawExitCode, Is.EqualTo(0));
            Assert.That(Repo.Log().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Commit_HookThatFails_DoesNotAllowCommit()
        {
            if (IsUsingPersistentClient)
            {
                Assert.Inconclusive("Hook tests does not function correctly under the persistent client");
                return;
            }

            Repo.SetHook("pre-commit", "fail");

            var command = new CustomCommand("commit")
                .WithAdditionalArgument("-m")
                .WithAdditionalArgument("dummy");
            Assert.Throws<MercurialExecutionException>(() => Repo.Execute(command));

            Assert.That(command.RawExitCode, Is.Not.EqualTo(0));
            Assert.That(Repo.Log().Count(), Is.EqualTo(0));
        }
    }
}
