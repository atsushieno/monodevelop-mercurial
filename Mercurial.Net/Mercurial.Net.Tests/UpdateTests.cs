using System;
using System.IO;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class UpdateTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void UpdateToNull_WithCommittedChanges_RemovesAddedFiles()
        {
            Repo.Init();
            string filePath = Path.Combine(Repo.Path, "test1.txt");
            File.WriteAllText(filePath, "dummy-content");
            Repo.Commit(
                "dummy", new CommitCommand
                {
                    AddRemove = true,
                });
            Repo.Update(RevSpec.Null);
            Assert.That(File.Exists(filePath), Is.False);
        }

        [Test]
        [Category("Integration")]
        public void UpdateWithRevisionParameter_ToNonExistantRevision_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            Assert.Throws<MercurialExecutionException>(() => Repo.Update(RevSpec.Single(2)));
        }

        [Test]
        [Category("Integration")]
        public void UpdateWithRevisionParameter_ToNullRevision_DoesNotThrowMercurialExecutionException()
        {
            Repo.Init();
            Repo.Update(RevSpec.Null);
        }

        [Test]
        [Category("Integration")]
        public void UpdateWithRevisionParameter_WithoutRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Update(RevSpec.Null));
        }

        [Test]
        [Category("Integration")]
        public void UpdateWithoutRevisionParameter_ToNonExistantRevision_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            Assert.Throws<MercurialExecutionException>(
                () => Repo.Update(
                    new UpdateCommand
                    {
                        Revision = RevSpec.Single(2)
                    }));
        }

        [Test]
        [Category("Integration")]
        public void UpdateWithoutRevisionParameter_ToNullRevision_DoesNotThrowMercurialExecutionException()
        {
            Repo.Init();
            Repo.Update(
                new UpdateCommand
                {
                    Revision = RevSpec.Null
                });
        }

        [Test]
        [Category("Integration")]
        public void UpdateWithoutRevisionParameter_WithoutRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(
                () => Repo.Update(
                    new UpdateCommand
                    {
                        Revision = RevSpec.Null
                    }));
        }

        [Test]
        [Category("API")]
        public void Update_NullRevision_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Repo.Update(null, null));
        }
    }
}