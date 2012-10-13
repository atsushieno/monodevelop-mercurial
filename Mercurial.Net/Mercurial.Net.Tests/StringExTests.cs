using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class StringExTests
    {
        [Test]
        [TestCase((string)null, true)]
        [TestCase("", true)]
        [TestCase(" \t\n\r ", true)]
        [TestCase(" \t\n\rx\t\n\r ", false)]
        [Category("Internal")]
        public void IsNullOrWhiteSpace_WithTestCases(string input, bool expected)
        {
            Assert.That(StringEx.IsNullOrWhiteSpace(input), Is.EqualTo(expected));
        }
    }
}