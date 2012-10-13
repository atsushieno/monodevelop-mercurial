using System;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class BookmarkTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("API")]
        public void Bookmark_NoRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Bookmark("Test"));
        }

        [Test]
        [Category("Integration")]
        public void Bookmark_NoNameSet_ThrowsInvalidOperationException()
        {
            Repo.Init();
            Assert.Throws<InvalidOperationException>(() => Repo.Bookmark(new BookmarkCommand()));
        }

        [Test]
        [Category("Integration")]
        public void Bookmark_CreatesNewBookmark()
        {
            Repo.Init();
            Repo.Bookmark("b1");
            CollectionAssert.AreEqual(
                new[]
                {
                    new Bookmark(-1, "b1")
                }, Repo.Bookmarks());
        }

        [Test]
        [Category("Integration")]
        public void BookmarkAndCommit_MovesBookmarkForward()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy1", "dummy1", true);
            Repo.Bookmark("b1");

            CollectionAssert.AreEqual(
                new[]
                {
                    new Bookmark(0, "b1"),
                }, Repo.Bookmarks());

            WriteTextFileAndCommit(Repo, "test.txt", "dummy2", "dummy2", true);

            CollectionAssert.AreEqual(
                new[]
                {
                    new Bookmark(1, "b1"),
                }, Repo.Bookmarks());
        }

        [Test]
        [Category("Integration")]
        public void Bookmark_MultipleBookmarks_CreatesCorrectSetOfBookmarks()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy1", "dummy1", true);
            WriteTextFileAndCommit(Repo, "test.txt", "dummy2", "dummy2", true);
            WriteTextFileAndCommit(Repo, "test.txt", "dummy3", "dummy3", true);
            Repo.Bookmark("b1", 0);
            Repo.Bookmark("b2", 1);
            Repo.Bookmark("b3", 2);

            CollectionAssert.AreEqual(
                new[]
                {
                    new Bookmark(0, "b1"),
                    new Bookmark(1, "b2"),
                    new Bookmark(2, "b3")
                }, Repo.Bookmarks());
        }
    }
}