using System;
using Mercurial.Versions;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class MercurialVersionAttributeTests
    {
        [Test]
        [Category("API")]
        [TestCase("", "0.0.0.0", "65535.65535.65535.65535")]
        [TestCase("1", "1.0.0.0", "1.65535.65535.65535")]
        [TestCase("1.2", "1.2.0.0", "1.2.65535.65535")]
        [TestCase("1.2.3", "1.2.3.0", "1.2.3.65535")]
        [TestCase("1.2.3.4", "1.2.3.4", "1.2.3.4")]
        public void VersionStringParsing_WithTestCases(string versionString, string expectedFromVersionString, string expectedToVersionString)
        {
            var attr = new MercurialVersionAttribute(versionString, versionString);

            Assert.That(attr.FromVersion, Is.EqualTo(new Version(expectedFromVersionString)));
            Assert.That(attr.ToVersion, Is.EqualTo(new Version(expectedToVersionString)));
        }

        [Test]
        [Category("API")]
        [TestCase("1.0.0.0", "2.0.0.0", "1.0.0.0", "2.0.0.0", 0)]
        [TestCase("1", "1", "2", "2", -1)] // 1 .. 1.* < 2 .. 2.*
        [TestCase("1", "1", "1.6", "1.6", +1)] // 1.6 .. 1.6.* < 1 .. 1.*
        [TestCase("1.6", "1.6", "1.6.2", "1.6.2", +1)] // 1.6.2 .. 1.6.2.* < 1.6 .. 1.6.*
        public void VersionStringOrdering_WithTestCases(string fromVersion1, string toVersion1, string fromVersion2, string toVersion2, int expectedOrdering)
        {
            var attr1 = new MercurialVersionAttribute(fromVersion1, toVersion1);
            var attr2 = new MercurialVersionAttribute(fromVersion2, toVersion2);

            int ordering = attr1.CompareTo(attr2);
            Assert.That(ordering, Is.EqualTo(expectedOrdering));

            ordering = attr2.CompareTo(attr1);
            Assert.That(ordering, Is.EqualTo(-expectedOrdering));
        }
    }
}