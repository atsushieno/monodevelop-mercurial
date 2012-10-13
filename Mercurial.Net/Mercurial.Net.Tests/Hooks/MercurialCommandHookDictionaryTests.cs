using System;
using System.Collections.Generic;
using Mercurial.Hooks;
using NUnit.Framework;

namespace Mercurial.Tests.Hooks
{
    [TestFixture]
    [Category("API")]
    public class MercurialCommandHookDictionaryTests
    {
        [Test]
        public void Constructor_NullArgumentsString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MercurialCommandHookDictionary(null));
        }

        [Test]
        public void Constructor_EmptyArgumentsString_ReturnsEmptyCollection()
        {
            var arguments = new MercurialCommandHookDictionary("{}");

            Assert.That(arguments.Count, Is.EqualTo(0));
        }

        [Test]
        [TestCase("{ }")]
        [TestCase("{\r}")]
        [TestCase("{\n}")]
        [TestCase("{\t}")]
        public void Constructor_WhitespaceArgumentsString_ThrowsInvalidOperationException(string input)
        {
            Assert.Throws<InvalidOperationException>(() => new MercurialCommandHookDictionary(input));
        }

        public IEnumerable<object[]> Constructor_TestCases()
        {
            yield return new object[]
            {
                "{'a': 'b'}",
                new Dictionary<string, object>
                {
                    { "a", "b" },
                },
            };

            yield return new object[]
            {
                "{'a': 'b', 'c': 'd'}",
                new Dictionary<string, object>
                {
                    { "a", "b" },
                    { "c", "d" },
                },
            };

            yield return new object[]
            {
                "{'a': None, 'c': 'd', 'e': None}",
                new Dictionary<string, object>
                {
                    { "a", null },
                    { "c", "d" },
                    { "e", null },
                },
            };

            yield return new object[]
            {
                "{'a': ['a', 'b', 'c', None]}",
                new Dictionary<string, object>
                {
                    { "a", new string[] { "a", "b", "c", null } },
                },
            };

            yield return new object[]
            {
                "{'a': ['a', 'b', 'c', None], 'b': None, 'c': [None, None, None]}",
                new Dictionary<string, object>
                {
                    { "a", new string[] { "a", "b", "c", null } },
                    { "b", null },
                    { "c", new string[] { null, null, null } },
                },
            };

            yield return new object[]
            {
                "{'a': 'a\\tb'}",
                new Dictionary<string, object>
                {
                    { "a", "a\tb" },
                },
            };

            yield return new object[]
            {
                "{'a': 'a\\rb'}",
                new Dictionary<string, object>
                {
                    { "a", "a\rb" },
                },
            };

            yield return new object[]
            {
                "{'a': 'a\\nb'}",
                new Dictionary<string, object>
                {
                    { "a", "a\nb" },
                },
            };
        }

        [Test]
        [TestCaseSource("Constructor_TestCases")]
        public void Constructor_WithTestCases(string input, Dictionary<string, object> expected)
        {
            var collection = new MercurialCommandHookDictionary(input);

            Assert.That(collection.Count, Is.EqualTo(expected.Count));
            foreach (string key in expected.Keys)
            {
                var value = expected[key];
                Assert.That(collection.ContainsKey(key));

                if (value == null)
                    Assert.That(collection[key], Is.Null);
                else if (value is string)
                    Assert.That(collection[key], Is.EqualTo(value));
                else if (value is string[])
                    CollectionAssert.AreEqual((string[])collection[key], (string[])value);
                else
                    Assert.Fail("key " + key + " has an unsupported value type");
            }
        }
    }
}
