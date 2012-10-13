using System.Globalization;
using NUnit.Framework;

namespace Mercurial.Tests.Hooks
{
    [TestFixture]
    [Category("Integration")]
    public class ChangegroupHookTests : DualRepositoryTestsBase
    {
        [Test]
        public void Pull_NoChangesets_DoesNotInvokeIncomingHook()
        {
            if (IsUsingPersistentClient)
            {
                Assert.Inconclusive("Hook tests does not function correctly under the persistent client");
                return;
            }

            Repo1.Init();
            Repo2.Clone(Repo1.Path);

            WriteTextFileAndCommit(Repo1, "test.txt", "dummy", "dummy", true);
            Repo2.Pull(Repo1.Path);

            Repo2.SetHook("changegroup");

            var command = new CustomCommand("pull")
                .WithAdditionalArgument(string.Format(CultureInfo.InvariantCulture, "\"{0}\"", Repo1.Path));
            Repo2.Execute(command);

            Assert.That(command.RawExitCode, Is.EqualTo(0));
            Assert.That(command.RawStandardOutput, Is.Not.StringContaining("Revision:"));
            Assert.That(command.RawStandardOutput, Is.Not.StringContaining("Url:"));
            Assert.That(command.RawStandardOutput, Is.Not.StringContaining("Source:"));
        }

        [Test]
        public void Pull_OneChangeset_InvokesIncomingHookOnce()
        {
            if (IsUsingPersistentClient)
            {
                Assert.Inconclusive("Hook tests does not function correctly under the persistent client");
                return;
            }

            Repo1.Init();
            Repo2.Clone(Repo1.Path);

            WriteTextFileAndCommit(Repo1, "test.txt", "dummy", "dummy", true);
            Repo2.SetHook("changegroup");

            var command = new CustomCommand("pull")
                .WithAdditionalArgument(string.Format(CultureInfo.InvariantCulture, "\"{0}\"", Repo1.Path));
            Repo2.Execute(command);

            Assert.That(command.RawExitCode, Is.EqualTo(0));
            Assert.That(command.RawStandardOutput.Count("Revision:"), Is.EqualTo(1));
            Assert.That(command.RawStandardOutput.Count("Url:"), Is.EqualTo(1));
            Assert.That(command.RawStandardOutput.Count("Source:"), Is.EqualTo(1));
        }

        [Test]
        public void Pull_TwoChangesets_InvokesIncomingHookOnce()
        {
            if (IsUsingPersistentClient)
            {
                Assert.Inconclusive("Hook tests does not function correctly under the persistent client");
                return;
            }

            Repo1.Init();
            Repo2.Clone(Repo1.Path);

            WriteTextFileAndCommit(Repo1, "test.txt", "dummy1", "dummy1", true);
            WriteTextFileAndCommit(Repo1, "test.txt", "dummy2", "dummy2", true);
            Repo2.SetHook("changegroup");

            var command = new CustomCommand("pull")
                .WithAdditionalArgument(string.Format(CultureInfo.InvariantCulture, "\"{0}\"", Repo1.Path));
            Repo2.Execute(command);

            Assert.That(command.RawExitCode, Is.EqualTo(0));
            Assert.That(command.RawStandardOutput.Count("Revision:"), Is.EqualTo(1));
            Assert.That(command.RawStandardOutput.Count("Url:"), Is.EqualTo(1));
            Assert.That(command.RawStandardOutput.Count("Source:"), Is.EqualTo(1));
        }
    }
}
