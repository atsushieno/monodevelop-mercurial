using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class ChangesetPathActionTests
    {
        [Test]
        [Category("API")]
        public void EqualsOfChangesetPathAction_BetweenTwoChangesetPathActionsWithTheSameContent_ReturnsTrue()
        {
            var cpa1 = new ChangesetPathAction
            {
                Action = ChangesetPathActionType.Add,
                Path = "/test.txt"
            };
            var cpa2 = new ChangesetPathAction
            {
                Action = ChangesetPathActionType.Add,
                Path = "/test.txt"
            };

            Assert.That(cpa1.Equals(cpa2), Is.True);
        }

        [Test]
        [Category("API")]
        public void EqualsOfObject_BetweenTwoChangesetPathActionsWithTheSameContent_ReturnsTrue()
        {
            var cpa1 = new ChangesetPathAction
            {
                Action = ChangesetPathActionType.Add,
                Path = "/test.txt"
            };
            var cpa2 = new ChangesetPathAction
            {
                Action = ChangesetPathActionType.Add,
                Path = "/test.txt"
            };

            Assert.That(cpa1.Equals((object)cpa2), Is.True);
        }

        [Test]
        [Category("API")]
        public void Equals_BetweenChangesetPathAction_AndObject_ReturnsFalse()
        {
            var cpa = new ChangesetPathAction
            {
                Action = ChangesetPathActionType.Add,
                Path = "/test.txt"
            };
            var obj = new object();

            Assert.That(cpa.Equals(obj), Is.False);
        }

        [Test]
        [Category("API")]
        public void GetHashCodeOfObjectsWithDifferentContents_ReturnsDifferentValues()
        {
            var cpa1 = new ChangesetPathAction
            {
                Action = ChangesetPathActionType.Add,
                Path = "/test1.txt"
            };
            var cpa2 = new ChangesetPathAction
            {
                Action = ChangesetPathActionType.Add,
                Path = "/test2.txt"
            };

            Assert.That(cpa1.GetHashCode(), Is.Not.EqualTo(cpa2.GetHashCode()));
        }

        [Test]
        [Category("API")]
        public void GetHashCodeOfObjectsWithSameContents_ReturnsSameValues()
        {
            var cpa1 = new ChangesetPathAction
            {
                Action = ChangesetPathActionType.Add,
                Path = "/test.txt"
            };
            var cpa2 = new ChangesetPathAction
            {
                Action = ChangesetPathActionType.Add,
                Path = "/test.txt"
            };

            Assert.That(cpa1.GetHashCode(), Is.EqualTo(cpa2.GetHashCode()));
        }

        [Test]
        [Category("API")]
        public void ToString_ReturnsActionAndPath()
        {
            var cpa = new ChangesetPathAction
            {
                Action = ChangesetPathActionType.Add,
                Path = "/test.txt"
            };
            string ts = cpa.ToString();

            Assert.That(ts, Is.EqualTo("Add: /test.txt"));
        }
    }
}