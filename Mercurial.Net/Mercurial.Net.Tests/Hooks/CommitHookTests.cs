using System;
using System.IO;
using NUnit.Framework;

namespace Mercurial.Tests.Hooks
{
    [TestFixture]
    [Category("Integration")]
    public class CommitHookTests : SingleRepositoryTestsBase
    {
        [Test]
        public void Commit_SingleChangeset_OutputsThatChangeset()
        {
            if (IsUsingPersistentClient)
            {
                Assert.Inconclusive("Hook tests does not function correctly under the persistent client");
                return;
            }

            Repo.Init();
            Repo.SetHook("commit");

            File.WriteAllText(Path.Combine(Repo.Path, "test.txt"), "dummy");
            Repo.Add("test.txt");

            var command = new CustomCommand("commit")
                .WithAdditionalArgument("-m")
                .WithAdditionalArgument("dummy");
            Repo.Execute(command);

            var tipHash = Repo.Tip().Hash;

            Assert.That(command.RawExitCode, Is.EqualTo(0));
            Assert.That(command.RawStandardOutput, Is.StringContaining("LeftParentRevision:0000000000000000000000000000000000000000"));
            Assert.That(command.RawStandardOutput, Is.StringContaining("RightParentRevision:" + Environment.NewLine));
            Assert.That(command.RawStandardOutput, Is.StringContaining("CommittedRevision:" + tipHash));
        }
    }
}