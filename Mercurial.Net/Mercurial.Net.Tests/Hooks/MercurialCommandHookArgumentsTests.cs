using System;
using System.Linq;
using Mercurial.Hooks;
using NUnit.Framework;

namespace Mercurial.Tests.Hooks
{
    [TestFixture]
    [Category("API")]
    public class MercurialCommandHookArgumentsTests
    {
        [Test]
        public void Constructor_NullArgumentsString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MercurialCommandHookArgumentsCollection(null));
        }

        [Test]
        public void Constructor_EmptyArgumentsString_ReturnsEmptyCollection()
        {
            var arguments = new MercurialCommandHookArgumentsCollection(string.Empty);

            Assert.That(arguments.Count, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WhitespaceArgumentsString_ReturnsEmptyCollection()
        {
            var arguments = new MercurialCommandHookArgumentsCollection(" \t\n\r ");

            Assert.That(arguments.Count, Is.EqualTo(0));
        }

        [Test]
        [TestCase("a", new string[] { "a" })]
        [TestCase("\"a\"", new string[] { "a" })]
        [TestCase("'a'", new string[] { "a" })]
        [TestCase("a b", new string[] { "a", "b" })]
        [TestCase("'a b'", new string[] { "a b" })]
        [TestCase("\"a b\"", new string[] { "a b" })]
        [TestCase("'a' 'b'", new string[] { "a", "b" })]
        [TestCase("\"a\" \"b\"", new string[] { "a", "b" })]
        [TestCase("abc def", new string[] { "abc", "def" })]
        [TestCase("a ", new string[] { "a" })]
        [TestCase(" a", new string[] { "a" })]
        public void Constructor_WithTestCases(string input, string[] expectedOutput)
        {
            var arguments = new MercurialCommandHookArgumentsCollection(input);

            CollectionAssert.AreEqual(arguments.ToArray(), expectedOutput);
        }
    }
}
