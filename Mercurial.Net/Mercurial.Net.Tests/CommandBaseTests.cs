using System;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class CommandBaseTests
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \r\t\n")]
        [Test]
        [Category("API")]
        public void WithConfigurationOverride_NullOrEmptySectionName_ThrowsArgumentNullException(string input)
        {
            var cmd = new DummyCommand();

            Assert.Throws<ArgumentNullException>(() => cmd.WithConfigurationOverride(input, "name", "value"));
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \r\t\n")]
        [Test]
        [Category("API")]
        public void WithConfigurationOverride_NullOrEmptyName_ThrowsArgumentNullException(string input)
        {
            var cmd = new DummyCommand();

            Assert.Throws<ArgumentNullException>(() => cmd.WithConfigurationOverride("section", input, "value"));
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \t\n\r")]
        [Test]
        [Category("API")]
        public void WithAdditionalArgument_NullOrEmptyArguments_ThrowsArgumentNullException(string input)
        {
            var cmd = new DummyCommand();
            Assert.Throws<ArgumentNullException>(() => cmd.WithAdditionalArgument(input));
        }

        [Test]
        [Category("API")]
        public void Constructor_ProducesEmptyAdditionalArguments()
        {
            var cmd = new DummyCommand();
            Assert.That(cmd.AdditionalArguments.Count, Is.EqualTo(0));
        }

        [Test]
        [Category("API")]
        public void WithAdditionalArgument_CorrectInputs_GetsAddedToAdditionalArguments()
        {
            var cmd = new DummyCommand();
            cmd.WithAdditionalArgument("--argument123");

            CollectionAssert.AreEqual(
                cmd.AdditionalArguments, new[]
                {
                    "--argument123",
                });
        }

        [Test]
        [Category("API")]
        public void WithConfigurationOverride_CorrectInputs_GetsAddedToAdditionalArguments()
        {
            var cmd = new DummyCommand();
            cmd.WithConfigurationOverride("section123", "name456", "value789");

            CollectionAssert.AreEqual(
                cmd.AdditionalArguments, new[]
                {
                    "--config", "section123.name456=\"value789\"",
                });
        }

        [Test]
        [Category("API")]
        public void WithConfigurationOverride_NullValue_ThrowsArgumentNullException()
        {
            var cmd = new DummyCommand();

            Assert.Throws<ArgumentNullException>(() => cmd.WithConfigurationOverride("section", "name", null));
        }
    }
}