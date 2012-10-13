using System;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class RevisionsTests
    {
        [TestCase(0, "0")]
        [TestCase(5, "5")]
        [TestCase(213, "213")]
        [Category("API")]
        [Test]
        public void ByRevision_WithZeroOrHigherRevisionNumbers_ReturnsRevisionNumberAsString(int revision, string expected)
        {
            string output = RevSpec.Single(revision).ToString();

            Assert.That(output, Is.EqualTo(expected));
        }

        [TestCase("default", "branch('default')")]
        [TestCase("hg-web-test", "branch('hg-web-test')")]
        [Category("API")]
        [Test]
        public void ByBranch_ForVariousTestCases_ProducesCorrectRevisionSpecification(string input, string expected)
        {
            RevSpec rev = RevSpec.ByBranch(input);
            string output = rev.ToString();

            Assert.That(output, Is.EqualTo(expected));
        }

        [TestCase("")]
        [TestCase((string)null)]
        [TestCase(" \t\n\r")]
        [Category("API")]
        [Test]
        public void Author_NullOrEmptyName_ThrowsArgumentNullException(string input)
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.Author(input));
        }

        [Test]
        [Category("API")]
        public void All_ReturnsAllFunction()
        {
            Assert.That(RevSpec.All.ToString(), Is.EqualTo("all()"));
        }

        [Test]
        [Category("API")]
        public void And_WithNullRevisions_ThrowsArgumentNullException()
        {
            RevSpec rev = RevSpec.Range(2, 7);
            Assert.Throws<ArgumentNullException>(() => rev.And(null));
        }

        [Test]
        [Category("API")]
        public void And_WithOtherRevisions_ProducesCorrectRevisionSpecification()
        {
            RevSpec rev1 = RevSpec.Range(2, 7);
            RevSpec rev2 = RevSpec.Range(10, 14);
            RevSpec result = rev1.And(rev2);
            Assert.That(result.ToString(), Is.EqualTo("2:7 and 10:14"));
        }

        [Test]
        [Category("API")]
        public void Author_WithName_ProducesCorrectRevisionSpecification()
        {
            RevSpec revSpec = RevSpec.Author("Lasse V. Karlsen <lasse@vkarlsen.no>");

            string output = revSpec.ToString();

            Assert.That(output, Is.EqualTo("author('Lasse V. Karlsen <lasse@vkarlsen.no>')"));
        }

        [Test]
        [Category("API")]
        public void Branches_ForRange_ProducesCorrectRevisionSpecification()
        {
            RevSpec revSpec = RevSpec.Range(2, 17);

            string output = revSpec.Branches.ToString();

            Assert.That(output, Is.EqualTo("branch(2:17)"));
        }

        [Test]
        [Category("API")]
        public void ByBranch_WithEmptyName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.ByBranch(string.Empty));
        }

        [Test]
        [Category("API")]
        public void ByBranch_WithNameSurroundedBySpaces_ReturnsTrimmedName()
        {
            string output = RevSpec.ByBranch(" stable ").ToString();
            const string expected = "branch('stable')";

            Assert.That(output, Is.EqualTo(expected));
        }

        [Test]
        [Category("API")]
        public void ByBranch_WithNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.ByBranch(null));
        }

        [Test]
        [Category("API")]
        public void ByBranch_WithWhitespaceName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.ByBranch(" \t \n \r "));
        }

        [Test]
        [Category("API")]
        public void ByHash_CorrectHashSurroundedByWhitespace_ReturnsTrimmedHash()
        {
            string output = RevSpec.Single(" 123abc ").ToString();
            const string expected = "123abc";

            Assert.That(output, Is.EqualTo(expected));
        }

        [Test]
        [Category("API")]
        public void ByHash_UppercaseHash_ReturnsLowercaseHash()
        {
            string output = RevSpec.Single("123ABC").ToString();
            const string expected = "123abc";

            Assert.That(output, Is.EqualTo(expected));
        }

        [Test]
        [Category("API")]
        public void ByHash_WithEmptyHash_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.Single(string.Empty));
        }

        [Test]
        [Category("API")]
        public void ByHash_WithInvalidHexDigits_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RevSpec.Single("x"));
        }

        [Test]
        [Category("API")]
        public void ByHash_WithNullHash_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.Single(null));
        }

        [Test]
        [Category("API")]
        public void ByHash_WithWhitespaceHash_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.Single("\t \t \r \n"));
        }

        [Test]
        [Category("API")]
        public void ByRevision_WithNegativeRevisionNumber_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RevSpec.Single(-1));
        }

        [Test]
        [Category("API")]
        public void ByTag_WithEmptyName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.ByTag(string.Empty));
        }

        [Test]
        [Category("API")]
        public void ByTag_WithNameSurroundedBySpaces_ReturnsTrimmedName()
        {
            string output = RevSpec.ByTag(" stable ").ToString();
            const string expected = "stable";

            Assert.That(output, Is.EqualTo(expected));
        }

        [Test]
        [Category("API")]
        public void ByTag_WithNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.ByTag(null));
        }

        [Test]
        [Category("API")]
        public void ByTag_WithWhitespaceName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.ByTag(" \t \n \r "));
        }

        [Test]
        [Category("API")]
        public void Children_OfRange_ProducesCorrectRevisionSpecification()
        {
            RevSpec revSpec = RevSpec.Range(2, 17);

            string output = revSpec.Children.ToString();

            Assert.That(output, Is.EqualTo("children(2:17)"));
        }

        [Test]
        [Category("API")]
        public void Closed_ReturnsClosedFunction()
        {
            Assert.That(RevSpec.Closed.ToString(), Is.EqualTo("closed()"));
        }

        [Test]
        [Category("API")]
        public void Except_ProducesCorrectRevisionSpecification()
        {
            RevSpec include = RevSpec.Range(2, 7);
            RevSpec exclude = RevSpec.Single(5);
            RevSpec result = include.Except(exclude);
            Assert.That(result.ToString(), Is.EqualTo("2:7 - 5"));
        }

        [Test]
        [Category("API")]
        public void Except_WithNullRevisions_ThrowsArgumentNullException()
        {
            RevSpec include = RevSpec.Range(2, 7);
            Assert.Throws<ArgumentNullException>(() => include.Except(null));
        }

        [Test]
        [Category("API")]
        public void From_WithNullRevision_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.From(null));
        }

        [Test]
        [Category("API")]
        public void From_WithRevisionByRevisionNumber_ProducesCorrectRevisionSpecification()
        {
            string output = RevSpec.From(2).ToString();
            const string expected = "2:";

            Assert.That(output, Is.EqualTo(expected));
        }

        [Test]
        [Category("API")]
        public void ImplicitConversion_NumberToRevisions_ProducesCorrectRevisionsValue()
        {
            RevSpec revSpec = 42;

            Assert.That(revSpec.ToString(), Is.EqualTo("42"));
        }

        [Test]
        [Category("API")]
        public void ImplicitConversion_RevisionsToString_ExtractsTheSpecification()
        {
            RevSpec revSpec = RevSpec.Range(17, 42);
            string value = revSpec;

            Assert.That(value, Is.EqualTo("17:42"));
        }

        [Test]
        [Category("API")]
        public void ImplicitConversion_StringToRevisions_ProducesCorrectRevisionsValue()
        {
            const string hash = "72beff810";
            RevSpec revSpec = hash;

            Assert.That(revSpec.ToString(), Is.EqualTo(hash));
        }

        [Test]
        [Category("API")]
        public void Not_OnNullRevisions_ThrowsArgumentNullException()
        {
            RevSpec set = null;

            Assert.Throws<ArgumentNullException>(
                () =>
                {
#pragma warning disable 168
                    RevSpec x = !set;
#pragma warning restore 168
                });
        }

        [Test]
        [Category("API")]
        public void Not_OnSimpleRange_CreatesCorrectExpression()
        {
            string output = (!RevSpec.Range(2, 7)).ToString();
            const string expected = "not 2:7";

            Assert.That(output, Is.EqualTo(expected));
        }

        [Test]
        [Category("API")]
        public void Null_ReturnsTheStringNullInLowercase()
        {
            Assert.That(RevSpec.Null.ToString(), Is.EqualTo("null"));
        }

        [Test]
        [Category("API")]
        public void Or_WithNullRevisions_ThrowsArgumentNullException()
        {
            RevSpec rev = RevSpec.Range(2, 7);
            Assert.Throws<ArgumentNullException>(() => rev.Or(null));
        }

        [Test]
        [Category("API")]
        public void Or_WithOtherRevisions_ProducesCorrectRevisionSpecification()
        {
            RevSpec rev1 = RevSpec.Range(2, 7);
            RevSpec rev2 = RevSpec.Range(10, 14);
            RevSpec result = rev1.Or(rev2);
            Assert.That(result.ToString(), Is.EqualTo("2:7 or 10:14"));
        }

        [Test]
        [Category("API")]
        public void Range_WithNullFromRevision_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.Range(null, 7));
        }

        [Test]
        [Category("API")]
        public void Range_WithNullToRevision_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.Range(2, null));
        }

        [Test]
        [Category("API")]
        public void Range_WithTwoRevisionsByRevisionNumber_ProducesCorrectRevisionSpecification()
        {
            string output = RevSpec.Range(2, 7).ToString();
            const string expected = "2:7";

            Assert.That(output, Is.EqualTo(expected));
        }

        [Test]
        [Category("API")]
        public void To_WithNullRevision_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RevSpec.To(null));
        }

        [Test]
        [Category("API")]
        public void To_WithRevisionByRevisionNumber_ProducesCorrectRevisionSpecification()
        {
            string output = RevSpec.To(7).ToString();
            const string expected = ":7";

            Assert.That(output, Is.EqualTo(expected));
        }

        [Test]
        [Category("API")]
        public void WorkingDirectoryParent_ReturnsAFullStop()
        {
            Assert.That(RevSpec.WorkingDirectoryParent.ToString(), Is.EqualTo("."));
        }

        [Test]
        [Category("API")]
        public void OperatorAnd_ReturnsCorrectExpression()
        {
            var r1 = new RevSpec("10");
            var r2 = new RevSpec("20");

            RevSpec r = r1 && r2;

            Assert.That(r, Is.EqualTo(new RevSpec("10 and 20")));
        }

        [Test]
        [Category("API")]
        public void OperatorOr_ReturnsCorrectExpression()
        {
            var r1 = new RevSpec("10");
            var r2 = new RevSpec("20");

            RevSpec r = r1 || r2;

            Assert.That(r, Is.EqualTo(new RevSpec("10 or 20")));
        }
    }
}