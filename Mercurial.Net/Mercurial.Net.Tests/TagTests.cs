using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class TagTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Tag_Changeset_AddsTagToChangeset()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy", "dummy", true);
            Repo.Tag("tagname");
            Changeset[] log = Repo.Log().ToArray();

            Assert.That(log.Length, Is.EqualTo(2));
            Assert.That(log[1].Tags.FirstOrDefault(), Is.EqualTo("tagname"));
        }

        [Test]
        [Category("Integration")]
        public void Tag_MultipleTagsForChangeset_ProducesLogWithAllTagsPresent()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy1", "dummy", true);
            Repo.Tag("test1", new TagCommand().WithRevision(0));
            Repo.Tag("test2", new TagCommand().WithRevision(0));

            Changeset[] log = Repo.Log().ToArray();

            Assert.That(log.Length, Is.EqualTo(3));
            CollectionAssert.AreEqual(
                log[2].Tags, new[]
                {
                    "test1", "test2"
                });
        }

        [Test]
        [Category("Integration")]
        public void Tag_RemoveTagFromChangesetWithTag_RemovesTag()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy1", "dummy", true);
            Repo.Tag("tagname");
            Changeset[] log = Repo.Log().ToArray();
            Assert.That(log[1].Tags.FirstOrDefault(), Is.EqualTo("tagname"));

            Repo.Tag(
                "tagname", new TagCommand
                {
                    Action = TagAction.Remove
                });

            log = Repo.Log().ToArray();
            Assert.That(log[2].Tags.Count(), Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public void Tag_RemoveTagWhenTagDoesNotExist_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy1", "dummy", true);

            Assert.Throws<MercurialExecutionException>(
                () => Repo.Tag(
                    "tagname", new TagCommand
                    {
                        Action = TagAction.Remove
                    }));
        }

        [Test]
        [Category("Integration")]
        public void Tag_UninitializedRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Tag("tagname"));
        }

        [Test]
        [Category("Integration")]
        public void Tag_WithExistingTagAndNotReplacingExisting_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy1", "dummy", true);
            Repo.Tag("tagname");

            Assert.Throws<MercurialExecutionException>(() => Repo.Tag("tagname"));
        }

        [Test]
        [Category("Integration")]
        public void Tag_WithExistingTagAndReplacingExisting_MovesTagToNewChangeset()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy1", "dummy", true);
            Repo.Tag("tagname");
            Repo.Tag(
                "tagname", new TagCommand
                {
                    ReplaceExisting = true
                });

            Changeset[] log = Repo.Log().ToArray();

            Assert.That(log.Length, Is.EqualTo(3));
            Assert.That(log[1].Tags.FirstOrDefault(), Is.EqualTo("tagname"));
            Assert.That(log[2].Tags.Count(), Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public void Tag_WithRevision_AppliesTagToCorrectChangeset()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy1", "dummy", true);
            WriteTextFileAndCommit(Repo, "test.txt", "dummy2", "dummy", false);
            Changeset[] log = Repo.Log().ToArray();

            Assert.That(log.Length, Is.EqualTo(2));
            RevSpec rev = log[1].Revision;

            Repo.Tag(
                "tagname", new TagCommand
                {
                    Revision = rev
                });

            log = Repo.Log().ToArray();

            Assert.That(log.Length, Is.EqualTo(3));
            Assert.That(log[2].Tags.FirstOrDefault(), Is.EqualTo("tagname"));
        }
    }
}