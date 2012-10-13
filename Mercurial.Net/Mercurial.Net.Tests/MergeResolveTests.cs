namespace Mercurial.Tests
{
    public class MergeResolveTests : SingleRepositoryTestsBase
    {
        protected const string BaseContents = "1\n2\n3\n4\n5";
        protected const string OtherChangedContents = "1 xxx\n2\n3\n4\n5";
        protected const string LocalChangedContentsWithoutConflict = "1\n2\n3\n4\n5 yyy";
        protected const string LocalChangedContentsWithConflict = "1 yyy\n2\n3\n4\n5";

        protected void CreateRepositoryWithoutMergeConflicts()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "dummy.txt", BaseContents, "dummy", true);
            WriteTextFileAndCommit(Repo, "dummy.txt", OtherChangedContents, "dummy", false);
            Repo.Update(0);
            WriteTextFileAndCommit(Repo, "dummy.txt", LocalChangedContentsWithoutConflict, "dummy", false);
        }

        protected void CreateRepositoryWithMergeConflicts()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "dummy.txt", BaseContents, "dummy", true);
            WriteTextFileAndCommit(Repo, "dummy.txt", OtherChangedContents, "dummy", false);
            Repo.Update(0);
            WriteTextFileAndCommit(Repo, "dummy.txt", LocalChangedContentsWithConflict, "dummy", false);
        }
    }
}