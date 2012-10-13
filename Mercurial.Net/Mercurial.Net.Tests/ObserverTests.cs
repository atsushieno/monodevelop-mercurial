using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class ObserverTests : SingleRepositoryTestsBase, IMercurialCommandObserver
    {
        #region Setup/Teardown

        public override void SetUp()
        {
            base.SetUp();

            _Output = new List<string>();
            _ErrorOutput = new List<string>();
            _ExecutingWasCalled = false;
            _ExecutedWasCalled = false;
        }

        #endregion

        private List<string> _Output;
        private List<string> _ErrorOutput;
        private bool _ExecutingWasCalled;
        private bool _ExecutedWasCalled;

        private void CallLogMethodWithObserver()
        {
            try
            {
                Repo.Log(
                    new LogCommand
                    {
                        Observer = this,
                    });
            }
            catch (MercurialExecutionException)
            {
                // Swallow this one
            }
        }

        public void Output(string line)
        {
            _Output.Add(line);
        }

        public void ErrorOutput(string line)
        {
            _ErrorOutput.Add(line);
        }

        public void Executing(string command, string arguments)
        {
            _ExecutingWasCalled = true;
        }

        public void Executed(string command, string arguments, int exitCode, string output, string errorOutput)
        {
            _ExecutedWasCalled = true;
        }

        [Test]
        [Category("Integration")]
        public void Log_AgainstInitializedRepositoryWithOneChangeset_ProducesStandardOutput()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "test1.txt"), "dummy content");
            Repo.Commit(
                "dummy", new CommitCommand
                {
                    AddRemove = true,
                });
            CallLogMethodWithObserver();

            Assert.That(_Output.Count, Is.GreaterThan(0));
        }

        [Test]
        [Category("Integration")]
        public void Log_AgainstUninitializedRepository_InvokesExecutedObserverMethod()
        {
            Repo.Init();
            CallLogMethodWithObserver();

            Assert.That(_ExecutedWasCalled, Is.True);
        }

        [Test]
        [Category("Integration")]
        public void Log_AgainstUninitializedRepository_InvokesExecutingObserverMethod()
        {
            Repo.Init();
            CallLogMethodWithObserver();

            Assert.That(_ExecutingWasCalled, Is.True);
        }

        [Test]
        [Category("Integration")]
        public void Log_AgainstUninitializedRepository_ProducesErrorOutput()
        {
            CallLogMethodWithObserver();

            Assert.That(_ErrorOutput.Count, Is.GreaterThan(0));
        }
    }
}