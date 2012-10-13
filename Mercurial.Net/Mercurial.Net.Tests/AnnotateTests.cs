using System;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class AnnotateTests : SingleRepositoryTestsBase
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase(" \r\n\t")]
        [Test]
        [Category("API")]
        public void Annotate_NullOrEmptyPath_ThrowsArgumentNullException(string input)
        {
            Assert.Throws<ArgumentNullException>(() => Repo.Annotate(input));
        }

        [Test]
        [Category("Integration")]
        public void Annotage_ForChangedFile_ReturnsAnnotationObjectWithPropertiesCorrectlySet()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "line 1\nline 2\nline 3", "dummy", true);
            WriteTextFileAndCommit(Repo, "test.txt", "line 1\nchanged in rev 1\nline 3", "dummy", false);
            WriteTextFileAndCommit(Repo, "test.txt", "line 1\nchanged in rev 1\nchanged in rev 2", "dummy", false);

            Annotation[] annotations = Repo.Annotate("test.txt").ToArray();
            Annotation annotation = annotations[0];

            Assert.That(annotation.LineNumber, Is.EqualTo(0));
            Assert.That(annotation.RevisionNumber, Is.EqualTo(0));
            Assert.That(annotation.Line, Is.EqualTo("line 1"));
        }

        [Test]
        [Category("Integration")]
        public void Annotate_ForChangedFile_ReturnsCorrectAnnotations()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "line 1\nline 2\nline 3", "dummy", true);
            WriteTextFileAndCommit(Repo, "test.txt", "line 1\nchanged in rev 1\nline 3", "dummy", false);
            WriteTextFileAndCommit(Repo, "test.txt", "line 1\nchanged in rev 1\nchanged in rev 2", "dummy", false);

            Annotation[] annotations = Repo.Annotate("test.txt").ToArray();
            CollectionAssert.AreEqual(
                annotations, new[]
                {
                    new Annotation(0, 0, "line 1"), new Annotation(1, 1, "changed in rev 1"), new Annotation(2, 2, "changed in rev 2"),
                });
        }

        [Test]
        [Category("Integration")]
        public void Annotate_InUninitializedRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Annotate("test.txt"));
        }

        [Test]
        [Category("API")]
        public void Validate_NoPath_ThrowsInvalidOperationException()
        {
            var command = new AnnotateCommand();

            Assert.Throws<InvalidOperationException>(command.Validate);
        }

        [Test]
        [Category("API")]
        public void Validate_WithPath_ReturnsWithoutThrowingException()
        {
            var command = new AnnotateCommand { Path = "test.txt" };

            command.Validate();
        }
    }
}