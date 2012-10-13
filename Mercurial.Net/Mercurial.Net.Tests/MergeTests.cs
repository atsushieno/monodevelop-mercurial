using System;
using System.IO;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class MergeTests : MergeResolveTests
    {
        [Test]
        [Category("API")]
        public void Merge_NoRepository_ThrowsMercurialExecutionException()
        {
            try
            {
                Repo.Merge(new MergeCommand().WithMergeTool(MergeTools.InternalMerge));
            }
            catch (MercurialExecutionException)
            {
                // Success
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }
        }

        [Test]
        [Category("Integration")]
        public void Merge_NothingToMergeWith_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "dummy.txt", "dummy", "dummy", true);

            try
            {
                Repo.Merge(new MergeCommand().WithMergeTool(MergeTools.InternalMerge));
            }
            catch (MercurialExecutionException)
            {
                // Success
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }
        }

        [Test]
        [Category("Integration")]
        public void Merge_TwoHeadsWithoutConflicts_ReturnsNoConflicts()
        {
            CreateRepositoryWithoutMergeConflicts();

            try
            {
                MergeResult result = Repo.Merge(new MergeCommand()
                    .WithMergeTool(MergeTools.InternalMerge));
                Assert.That(result, Is.EqualTo(MergeResult.Success));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }
        }

        [Test]
        [Category("Integration")]
        public void Merge_TwoHeadsWithSingleConflict_IndicatesConflict()
        {
            CreateRepositoryWithMergeConflicts();

            try
            {
                try
                {
                    MergeResult result = Repo.Merge(new MergeCommand()
                        .WithMergeTool(MergeTools.InternalMerge));
                    Assert.That(result, Is.EqualTo(MergeResult.UnresolvedFiles));
                }
                catch (NotSupportedException)
                {
                    Assert.Inconclusive("Merge tool not supported in this version");
                }
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }
        }

        [Test]
        [Category("Integration")]
        public void Merge_InternalLocalMergeTool_ResolvesAndPickLocalContents()
        {
            CreateRepositoryWithMergeConflicts();

            try
            {
                MergeResult result = Repo.Merge(new MergeCommand()
                    .WithMergeTool(MergeTools.InternalLocal));
                Assert.That(result, Is.EqualTo(MergeResult.Success));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }

            string contents = File.ReadAllText(Path.Combine(Repo.Path, "dummy.txt"));

            Assert.That(contents, Is.EqualTo(LocalChangedContentsWithConflict));
        }

        [Test]
        [Category("Integration")]
        public void Merge_InternalOtherMergeTool_ResolvesAndPickOtherContents()
        {
            CreateRepositoryWithMergeConflicts();

            try
            {
                MergeResult result = Repo.Merge(new MergeCommand()
                    .WithMergeTool(MergeTools.InternalOther));
                string contents = File.ReadAllText(Path.Combine(Repo.Path, "dummy.txt"));

                Assert.That(result, Is.EqualTo(MergeResult.Success));
                Assert.That(contents, Is.EqualTo(OtherChangedContents));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }
        }
    }
}