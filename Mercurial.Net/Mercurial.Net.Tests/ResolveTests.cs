using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class ResolveTests : MergeResolveTests
    {
        [Test]
        [Category("Integration")]
        public void Resolve_NoRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Resolve("xyz"));
        }

        [Test]
        [Category("Integration")]
        public void Resolve_NoPendingMerge_ReturnsNoConflicts()
        {
            Repo.Init();

            IEnumerable<MergeConflict> conflicts = Repo.Resolve(new ResolveCommand().WithAction(ResolveAction.List));

            Assert.That(conflicts, Is.Empty.And.Not.Null);
        }

        [Test]
        [Category("Integration")]
        public void Resolve_AfterMergeWithNoConflicts_ReturnsResolvedConflicts()
        {
            CreateRepositoryWithoutMergeConflicts();
            try
            {
                Repo.Merge(new MergeCommand()
                    .WithMergeTool(MergeTools.InternalMerge));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }

            IEnumerable<MergeConflict> conflicts = Repo.Resolve(new ResolveCommand().WithAction(ResolveAction.List));

            CollectionAssert.AreEqual(
                new[]
                {
                    new MergeConflict("dummy.txt", MergeConflictState.Resolved),
                }, conflicts);
        }

        [Test]
        [Category("Integration")]
        public void Resolve_AfterMergeWithConflict_ReturnsTheConflict()
        {
            CreateRepositoryWithMergeConflicts();

            try
            {
                Repo.Merge(new MergeCommand()
                    .WithMergeTool(MergeTools.InternalMerge));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }

            IEnumerable<MergeConflict> conflicts = Repo.Resolve(new ResolveCommand().WithAction(ResolveAction.List));

            CollectionAssert.AreEqual(
                new[]
                {
                    new MergeConflict("dummy.txt", MergeConflictState.Unresolved),
                }, conflicts);
        }

        [Test]
        [Category("Integration")]
        public void Resolve_MarkFileAsResolved_MarksFileAsResolved()
        {
            CreateRepositoryWithMergeConflicts();

            try
            {
                Repo.Merge(new MergeCommand()
                    .WithMergeTool(MergeTools.InternalMerge));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }

            Repo.Resolve("dummy.txt");

            IEnumerable<MergeConflict> conflicts = Repo.Resolve(new ResolveCommand().WithAction(ResolveAction.List));
            CollectionAssert.AreEqual(
                new[]
                {
                    new MergeConflict("dummy.txt", MergeConflictState.Resolved),
                }, conflicts);
        }

        [Test]
        [Category("Integration")]
        public void Resolve_MarkFileAsUnresolvedAfterMarkingAsResolved_MarksFileAsUnresolved()
        {
            CreateRepositoryWithMergeConflicts();

            try
            {
                Repo.Merge(new MergeCommand()
                    .WithMergeTool(MergeTools.InternalMerge));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }

            Repo.Resolve("dummy.txt");
            Repo.Resolve("dummy.txt", ResolveAction.MarkUnresolved);

            IEnumerable<MergeConflict> conflicts = Repo.Resolve(new ResolveCommand().WithAction(ResolveAction.List));
            CollectionAssert.AreEqual(
                new[]
                {
                    new MergeConflict("dummy.txt", MergeConflictState.Unresolved),
                }, conflicts);
        }

        [Test]
        [Category("Integration")]
        public void Resolve_MarkAllFilesAsResolved_MarksFileAsResolved()
        {
            CreateRepositoryWithMergeConflicts();

            try
            {
                Repo.Merge(new MergeCommand()
                    .WithMergeTool(MergeTools.InternalMerge));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }

            Repo.Resolve(new ResolveCommand().WithAction(ResolveAction.MarkResolved).WithSelectAll());

            IEnumerable<MergeConflict> conflicts = Repo.Resolve(new ResolveCommand().WithAction(ResolveAction.List));
            CollectionAssert.AreEqual(
                new[]
                {
                    new MergeConflict("dummy.txt", MergeConflictState.Resolved),
                }, conflicts);
        }
    }
}