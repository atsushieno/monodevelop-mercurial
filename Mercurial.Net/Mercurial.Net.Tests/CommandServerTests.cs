using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    [Ignore]
    public class CommandServerTests : SingleRepositoryTestsBase
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Repo.Init();
        }

        private int[] MercurialProcessIDs()
        {
            var ids = new List<int>();
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.Modules.Cast<ProcessModule>().Any(module => string.Compare(Path.GetFileName(module.FileName), "HG.EXE", StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        ids.Add(process.Id);
                    }
                }
                catch (Win32Exception ex)
                {
                    switch (ex.NativeErrorCode)
                    {
                        case 299: // 32-bit vs. 64-bit process conflict
                        case 5: // access denied
                        case -2147467259: // unable to enumerate the process modules
                            // swallow this one
                            continue;

                        default:
                            throw;
                    }
                }
                catch (InvalidOperationException)
                {
                    // race condition, process terminated while we were enumerating list of processes
                    // swallow this one
                }
            }

            return ids.ToArray();
        }

        [Test]
        [NUnit.Framework.Category("Integration")]
        public void ConstructingRepositoryObject_WithPersistentClient_SpinsUpExactlyOneClient()
        {
            if (!IsUsingPersistentClient)
            {
                Assert.Inconclusive("Not testing the persistent client");
                return;
            }

            var before = MercurialProcessIDs();
            using (var repo = new Repository(Repo.Path))
            {
                var after = MercurialProcessIDs();

                Assert.That(after.Length, Is.EqualTo(before.Length + 1));
            }
        }

        [Test]
        [NUnit.Framework.Category("Integration")]
        public void DisposingRepositoryObject_WithPersistentClient_TerminatesExactlyOneClient()
        {
            if (!IsUsingPersistentClient)
            {
                Assert.Inconclusive("Not testing the persistent client");
                return;
            }

            int[] before;
            using (var repo = new Repository(Repo.Path))
            {
                before = MercurialProcessIDs();
            }
            var after = MercurialProcessIDs();

            Assert.That(after.Length, Is.EqualTo(before.Length - 1));
        }

        [Test]
        [NUnit.Framework.Category("Integration")]
        public void GarbageCollectRepositoryObject_WithPersistentClient_TerminatesExactlyOneClient()
        {
            if (!IsUsingPersistentClient)
            {
                Assert.Inconclusive("Not testing the persistent client");
                return;
            }

            var before = MercurialProcessIDs();
            CreateRepositoryObject();
            var after = MercurialProcessIDs();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var final = MercurialProcessIDs();

            Assert.That(after.Length, Is.EqualTo(before.Length + 1));
            Assert.That(final.Length, Is.EqualTo(before.Length));
        }

        private void CreateRepositoryObject()
        {
            var repo = new Repository(Repo.Path);
        }
    }
}
