namespace Mercurial.Tests
{
    internal class DummyCommand : MercurialCommandBase<DummyCommand>
    {
        public DummyCommand()
            : base("dummy")
        {
        }
    }
}