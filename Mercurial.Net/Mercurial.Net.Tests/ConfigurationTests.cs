using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class ConfigurationTests
    {
        [TestCase("ui")]
        [TestCase("extensions")]
        [Test]
        [Category("Integration")]
        public void Sections_ReturnsCollectionOfSectionNames_ContainingAtLeastAFewOfTheDefaultOnes(string sectionName)
        {
            IEnumerable<string> sectionNames = ClientExecutable.Configuration.Sections;

            Assert.That(sectionNames.Contains(sectionName), Is.True);
        }

        [TestCase("ui", "username")]
        [TestCase("extensions", "graphlog")]
        [Test]
        [Category("Integration")]
        public void Entries_ForSpecifiedTypicalOnes_Exists(string sectionName, string name)
        {
            Assert.That(ClientExecutable.Configuration.ValueExists(sectionName, name), Is.True);
        }

        [TestCase("dummysection", "username")]
        [TestCase("ui", "dummyname")]
        [Test]
        [Category("Integration")]
        public void Entries_ForRandomlyGeneratedNames_DoesNotExist(string sectionName, string name)
        {
            Assert.That(ClientExecutable.Configuration.ValueExists(sectionName, name), Is.False);
        }
    }
}