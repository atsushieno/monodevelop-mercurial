using NUnit.Framework;

namespace Mercurial.Tests
{
    public abstract class DualRepositoryTestsBase : RepositoryTestsBase
    {
        protected Repository Repo1
        {
            get;
            private set;
        }

        protected Repository Repo2
        {
            get;
            private set;
        }

        [SetUp]
        public virtual void SetUp()
        {
            Repo1 = GetRepository();
            Repo2 = GetRepository();
        }

        [TearDown]
        public override void TearDown()
        {
            Repo1.Dispose();
            Repo1 = null;

            Repo2.Dispose();
            Repo2 = null;

            base.TearDown();
        }
    }
}