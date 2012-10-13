using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class PhaseTests : DualRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Phase_OfNonExistantChangeset_ThrowsMercurialExecutionException()
        {
            if (ClientExecutable.CurrentVersion < new Version(2, 1))
                Assert.Inconclusive("The phase command is not present in this Mercurial version");

            Repo1.Init();

            Assert.Throws<MercurialExecutionException>(() => Repo1.Phase("600").ToArray());
        }

        [Test]
        [Category("Integration")]
        public void Phase_OfNewChangeset_ReturnsDraft()
        {
            if (ClientExecutable.CurrentVersion < new Version(2, 1))
                Assert.Inconclusive("The phase command is not present in this Mercurial version");

            Repo1.Init();
            File.WriteAllText(Path.Combine(Repo1.Path, "test1.txt"), "dummy content");
            Repo1.AddRemove();
            Repo1.Commit("Test");

            ChangesetPhase[] phases = null;
            try
            {
                phases = Repo1.Phase("0").ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            CollectionAssert.AreEqual(new[]
            {
                new ChangesetPhase(0, Phases.Draft),
            }, phases);
        }

        [Test]
        [Category("Integration")]
        public void Phase_OfPushedChangeset_ChangesToPublic()
        {
            if (ClientExecutable.CurrentVersion < new Version(2, 1))
                Assert.Inconclusive("The phase command is not present in this Mercurial version");

            Repo1.Init();
            File.WriteAllText(Path.Combine(Repo1.Path, "test1.txt"), "dummy content");
            Repo1.AddRemove();
            Repo1.Commit("Test");

            Repo2.Init();
            Repo1.Push(Repo2.Path);

            ChangesetPhase[] phases = null;
            try
            {
                phases = Repo1.Phase("0").ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            CollectionAssert.AreEqual(new[]
            {
                new ChangesetPhase(0, Phases.Public),
            }, phases);
        }
    }
}
