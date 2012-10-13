using System;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class CopyTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Copy_EverythingOK_RecordsCopy()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test1.txt", "dummy", "dummy", true);
            Repo.Copy("test1.txt", "test2.txt");
            Repo.Commit("dummy2");

            Changeset[] log = Repo.Log(new LogCommand().WithIncludePathActions()).ToArray();

            Assert.That(log.Length, Is.EqualTo(2));
            CollectionAssert.AreEqual(
                log[0].PathActions, new[]
                {
                    new ChangesetPathAction
                    {
                        Action = ChangesetPathActionType.Add,
                        Path = "test2.txt",
                        Source = "test1.txt",
                    }
                });
        }

        [Test]
        [Category("Integration")]
        public void Copy_NoRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Copy("test1.txt", "test2.txt"));
        }

        [Test]
        [Category("Integration")]
        public void Copy_SourceDoesNotExist_ThrowsMercurialExecutionException()
        {
            Repo.Init();

            Assert.Throws<MercurialExecutionException>(() => Repo.Copy("test1.txt", "test2.txt"));
        }

        [Test]
        [Category("API")]
        public void Destination_Blank_DoesNotValidate()
        {
            var command = new CopyCommand();

            Assert.Throws<InvalidOperationException>(command.Validate);
        }

        [Test]
        [Category("API")]
        public void Destination_NonBlank_DoesValidate()
        {
            var command = new CopyCommand { Destination = "test2.txt" };

            command.Validate();
        }
    }
}