using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class ChangesetTests
    {
        [Test]
        [Category("API")]
        public void Equals_ChangesetAndObject_ReturnsFalse()
        {
            var set = new Changeset();
            var obj = new object();

            Assert.That(set.Equals(obj), Is.False);
        }
    }
}