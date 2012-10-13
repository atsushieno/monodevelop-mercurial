using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class InitTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Init_WithInitializedRepo_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            Assert.Throws<MercurialExecutionException>(() => Repo.Init());
        }
    }
}