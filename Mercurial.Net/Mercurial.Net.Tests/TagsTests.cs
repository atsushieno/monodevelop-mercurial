using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class TagsTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Tags_NoRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Tags());
        }

        [Test]
        [Category("Integration")]
        public void Tags_InitializeRepository_ReturnsTip()
        {
            Repo.Init();

            Tag[] tags = Repo.Tags().ToArray();

            CollectionAssert.AreEqual(
                new[]
                {
                    new Tag(-1, "tip")
                }, tags);
        }

        [Test]
        [Category("Integration")]
        public void Tags_MultipleTags_ReportTagsCorrectly()
        {
            Repo.Init();

            WriteTextFileAndCommit(Repo, "test1.txt", "dummy1", "dummy1", true);
            WriteTextFileAndCommit(Repo, "test1.txt", "dummy2", "dummy2", false);

            Repo.Tag(new TagCommand().WithRevision(0).WithName("t1"));
            Repo.Tag(new TagCommand().WithRevision(1).WithName("t2"));

            Tag[] tags = Repo.Tags().ToArray();

            CollectionAssert.AreEqual(
                new[]
                {
                    new Tag(0, "t1"),
                    new Tag(1, "t2"),
                    new Tag(3, "tip")
                }, tags);
        }
    }
}