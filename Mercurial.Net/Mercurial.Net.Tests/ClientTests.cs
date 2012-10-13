using System;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class ClientTests
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \t\n\r")]
        [Test]
        [Category("API")]
        public void Execute_NullOrEmptyRepositoryPath_ThrowsArgumentNullException(string input)
        {
            var command = new DummyCommand();

            Assert.Throws<ArgumentNullException>(() => NonPersistentClient.Execute(input, command));
        }

        [Test]
        [Category("API")]
        public void Execute_NullCommand_ThrowsArgumentNullException()
        {
            IMercurialCommand command = null;
            Assert.Throws<ArgumentNullException>(() => NonPersistentClient.Execute(command));
        }

        [Test]
        [Category("Integration")]
        public void GetHelpText_ForLogCommand_ReturnsText()
        {
            var command = new HelpCommand
            {
                Topic = "log"
            };
            NonPersistentClient.Execute(command);
            string text = command.Result;

            Assert.That(text, Is.StringContaining("show revision history of entire repository or files"));
        }

        [Test]
        [Category("Integration")]
        public void GetHelpText_WithGlobalOptions_IncludesGlobalOptions()
        {
            var command = new HelpCommand
            {
                Topic = "log",
                IncludeGlobalHelp = true
            };
            NonPersistentClient.Execute(command);
            string text = command.Result;

            Assert.That(text, Is.StringContaining("enable debugging output"));
        }

        [Test]
        [Category("Integration")]
        public void GetHelpText_WithoutGlobalOptions_DoesNotIncludeGlobalOptions()
        {
            var command = new HelpCommand
            {
                Topic = "log"
            };
            NonPersistentClient.Execute(command);
            string text = command.Result;

            Assert.That(text, Is.Not.StringContaining("enable debugging output"));
        }

        [Test]
        [Category("API")]
        public void SupportedVersions_IsIncreasing()
        {
            Version[] supportedVersions = ClientExecutable.SupportedVersions.ToArray();

            CollectionAssert.IsOrdered(supportedVersions);
        }

        [Test]
        [Category("Integration")]
        public void Version_IsOneOfTheSupportedOnes()
        {
            Assert.That(
                new[]
                {
                    ClientExecutable.GetVersion()
                }, Is.SubsetOf(ClientExecutable.SupportedVersions));
        }
    }
}