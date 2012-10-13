// 
// MercurialRepository.cs
//  
// Author:
//       Atsushi Eno <atsushi@xamarin.com>
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

//#define DEBUG_GIT

using System;
using System.Linq;
using System.IO;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using MonoDevelop.Ide;
using Mercurial;
using Mono.TextEditor;

using DiffEntry = System.String; // diffs in Mercurial.NET are mere strings.

namespace MonoDevelop.VersionControl.Mercurial
{
	public class MercurialRepository : UrlBasedRepository
	{
		global::Mercurial.Repository  repo;
		FilePath path;
		static readonly byte[] EmptyContent = new byte[0];
		
		public static event EventHandler BranchSelectionChanged;
		
		public MercurialRepository ()
		{
			Url = "https://";
		}
		
		public MercurialRepository (FilePath path, string url)
		{
			this.path = path;
			Url = url;
			repo = new global::Mercurial.Repository (path, new global::Mercurial.PersistentClientFactory ());
		}
		
		public override void Dispose ()
		{
			((MercurialVersionControl)VersionControlSystem).UnregisterRepo (this);
			base.Dispose ();
		}
		
		public override string[] SupportedProtocols {
			get {
				// To support ftp/ftps, FTPExtension support is needed in Mercurial.net
				return new string[] {"ssh", "http", "https", "rsync", "file"};
			}
		}
		
		public override string[] SupportedNonUrlProtocols {
			get {
				return new string[] {"ssh/scp"};
			}
		}
		
		public override bool IsUrlValid (string url)
		{
			try {
				// mercurial-FIXME: implement.
				//new global::Mercurial.RemoteRepositoryPath (url);
				return true;
			} catch {
			}
			return base.IsUrlValid (url);
		}
		
		public override string Protocol {
			get {
				string p = base.Protocol;
				if (p != null)
					return p;
				return IsUrlValid (Url) ? "ssh/scp" : null;
			}
		}
		
		public FilePath RootPath {
			get { return path; }
		}
		
		public override void CopyConfigurationFrom (Repository other)
		{
			base.CopyConfigurationFrom (other);
			MercurialRepository r = (MercurialRepository)other;
			path = r.path;
			if (r.repo != null)
				repo = new global::Mercurial.Repository (path);
		}
		
		public override string LocationDescription {
			get { return Url ?? path; }
		}
		
		public override bool AllowLocking {
			get { return false; }
		}
		
		public override string GetBaseText (FilePath localFile)
		{
			Changeset c = repo.Tip ();
			if (c == null)
				return string.Empty;
			var stat = repo.Status (new StatusCommand ().WithAdditionalArgument (localFile)).FirstOrDefault ();
			if (stat != null && stat.State == FileState.Added)
				return string.Empty;
			return repo.Cat (ToMercurialPath (localFile), new CatCommand ());
		}

		// mercurial-FIXME: GetStashes() not supported

		public override Revision[] GetHistory (FilePath localFile, Revision since)
		{
			List<Revision> revs = new List<Revision> ();
			var commits = repo.Log (new global::Mercurial.LogCommand ().WithIncludePattern (ToMercurialPath (localFile)));
			foreach (var commit in commits) {
				MercurialRevision rev = new MercurialRevision (this, commit.Hash, commit.Timestamp, commit.AuthorName, commit.CommitMessage) {
					Email = commit.AuthorEmailAddress,
					ShortMessage = commit.CommitMessage.Split (line_sep).FirstOrDefault (),
					Commit = commit,
					FileForChanges = localFile
				};
				revs.Add (rev);
			}
			return revs.ToArray ();
		}
		static readonly char [] line_sep = new char [] {'\n'};
		
		protected override RevisionPath[] OnGetRevisionChanges (Revision revision)
		{
			MercurialRevision rev = (MercurialRevision) revision;
			if (rev.Commit == null)
				return new RevisionPath [0];
			
			List<RevisionPath> paths = new List<RevisionPath> ();
			foreach (var entry in rev.Commit.PathActions) {
				if (entry.Action == ChangesetPathActionType.Add)
					paths.Add (new RevisionPath (FromMercurialPath (entry.Path), RevisionAction.Add, null));
				if (entry.Action == ChangesetPathActionType.Remove)
					paths.Add (new RevisionPath (FromMercurialPath (entry.Path), RevisionAction.Delete, null));
				if (entry.Action == ChangesetPathActionType.Modify)
					paths.Add (new RevisionPath (FromMercurialPath (entry.Path), RevisionAction.Modify, null));
			}
			return paths.ToArray ();
		}
		
		
		protected override IEnumerable<VersionInfo> OnGetVersionInfo (IEnumerable<FilePath> paths, bool getRemoteStatus)
		{
			return GetDirectoryVersionInfo (FilePath.Null, paths, getRemoteStatus, false);
		}
		
		protected override VersionInfo[] OnGetDirectoryVersionInfo (FilePath localDirectory, bool getRemoteStatus, bool recursive)
		{
			return GetDirectoryVersionInfo (localDirectory, null, getRemoteStatus, recursive);
		}
		
		string ToMercurialPath (FilePath filePath)
		{
			if (!filePath.IsAbsolute)
				return filePath;
			return filePath.FullPath.ToRelative (path).ToString ().Replace ('\\', '/');
		}
		
		VersionInfo[] GetDirectoryVersionInfo (FilePath localDirectory, IEnumerable<FilePath> localFileNames, bool getRemoteStatus, bool recursive)
		{
			List<VersionInfo> versions = new List<VersionInfo> ();
			HashSet<FilePath> existingFiles = new HashSet<FilePath> ();
			HashSet<FilePath> nonVersionedMissingFiles = new HashSet<FilePath> ();
			
			if (localFileNames != null) {
				var localFiles = new List<FilePath> ();
				var arev = new MercurialRevision (this, "");
				foreach (var p in localFileNames) {
					if (Directory.Exists (p)) {
						if (recursive)
							versions.AddRange (GetDirectoryVersionInfo (p, getRemoteStatus, true));
						else
							versions.Add (new VersionInfo (p, "", true, VersionStatus.Versioned, arev, VersionStatus.Versioned, null));
					}
					else {
						localFiles.Add (p);
						if (File.Exists (p))
							existingFiles.Add (p.CanonicalPath);
						else
							nonVersionedMissingFiles.Add (p.CanonicalPath);
					}
				}
				// No files to check, we are done
				if (localFiles.Count == 0)
					return versions.ToArray ();
				
				localFileNames = localFiles;
			} else {
				CollectFiles (existingFiles, localDirectory, recursive);
			}
			
			MercurialRevision rev;
			var headCommit = repo.Tip ();
			if (headCommit != null)
				rev = new MercurialRevision (this, headCommit.Hash);
			else
				rev = null;
			
			IEnumerable<string> paths;
			if (localFileNames == null) {
				if (recursive)
				paths = new [] { (string) localDirectory };
				else
					paths = Directory.GetFiles (localDirectory);
			} else {
				paths = localFileNames.Select (f => (string)f);
			}
			paths = paths.Select (f => ToMercurialPath (f));

			var statcmd = new StatusCommand ();
			foreach (var p in paths) statcmd.AddArgument (p);
			var status = repo.Status (statcmd);
			HashSet<string> added = new HashSet<string> ();
			Action<IEnumerable<FileStatus>, VersionStatus> AddFiles = delegate(IEnumerable<FileStatus> files, VersionStatus fstatus) {
				foreach (FileStatus file in files) {
					if (!added.Add (file.Path))
						continue;
					FilePath statFile = FromMercurialPath (file.Path);
					existingFiles.Remove (statFile.CanonicalPath);
					nonVersionedMissingFiles.Remove (statFile.CanonicalPath);
					versions.Add (new VersionInfo (statFile, "", false, fstatus, rev, VersionStatus.Versioned, null));
				}
			};
			
			AddFiles (status.Where (fs => fs.State == FileState.Added), VersionStatus.Versioned | VersionStatus.ScheduledAdd);
			AddFiles (status.Where (fs => fs.State == FileState.Modified), VersionStatus.Versioned | VersionStatus.Modified);
			AddFiles (status.Where (fs => fs.State == FileState.Removed), VersionStatus.Versioned | VersionStatus.ScheduledDelete);
			AddFiles (status.Where (fs => fs.State == FileState.Missing), VersionStatus.Versioned | VersionStatus.ScheduledDelete);
			// mercurial-FIXME: how can I get conflicts?
			//AddFiles (status.GetConflicting (), VersionStatus.Versioned | VersionStatus.Conflicted);
			AddFiles (status.Where (fs => fs.State == FileState.Unknown), VersionStatus.Unversioned);
			
			// Existing files for which hg did not report an status are supposed to be tracked
			foreach (FilePath file in existingFiles) {
				VersionInfo vi = new VersionInfo (file, "", false, VersionStatus.Versioned, rev, VersionStatus.Versioned, null);
				versions.Add (vi);
			}
			
			// Non existing files for which hg did not report an status are unversioned
			foreach (FilePath file in nonVersionedMissingFiles)
				versions.Add (VersionInfo.CreateUnversioned (file, false));
			
			return versions.ToArray ();
		}
		
		protected override VersionControlOperation GetSupportedOperations (VersionInfo vinfo)
		{
			VersionControlOperation ops = base.GetSupportedOperations (vinfo);
			if (GetCurrentRemote () == null)
				ops &= ~VersionControlOperation.Update;
			if (vinfo.IsVersioned && !vinfo.IsDirectory)
				ops |= VersionControlOperation.Annotate;
			return ops;
		}
		
		void CollectFiles (HashSet<FilePath> files, FilePath dir, bool recursive)
		{
			foreach (string file in Directory.GetFiles (dir))
				files.Add (new FilePath (file).CanonicalPath);
			if (recursive) {
				foreach (string sub in Directory.GetDirectories (dir))
					CollectFiles (files, sub, true);
			}
		}
		
		public override Repository Publish (string serverPath, FilePath localPath, FilePath[] files, string message, IProgressMonitor monitor)
		{
			throw new NotImplementedException ();
			/*
			// Initialize the repository
			repo = new global::Mercurial.Repository (ToMercurialPath (localPath));

			GitUtil.Init (localPath, Url, monitor);
			NGit.Api.Git git = new NGit.Api.Git (repo);
			try {
				var refs = git.Fetch ().Call ().GetAdvertisedRefs ();
				if (refs.Count > 0) {
					throw new UserException ("The remote repository already contains branches. MonoDevelop can only publish to an empty repository");
				}
			} catch {
				if (Directory.Exists (repo.Directory))
					Directory.Delete (repo.Directory, true);
				repo.Close ();
				repo = null;
				throw;
			}
			
			path = localPath;
			// Add the project files
			ChangeSet cs = CreateChangeSet (localPath);
			var cmd = git.Add ();
			foreach (FilePath fp in files) {
				cmd.AddFilepattern (ToMercurialPath (fp));
				cs.AddFile (fp);
			}
			cmd.Call ();
			
			// Create the initial commit
			cs.GlobalComment = message;
			Commit (cs, monitor);
			
			// Push to remote repo
			Push (monitor, "origin", "master");
			
			return this;
			*/
		}

		// no way to support localPaths/recurse; ignore.
		public override void Update (FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
		{
			repo.Update (new global::Mercurial.UpdateCommand ());
			/*
			IEnumerable<DiffEntry> statusList = null;
			
			monitor.BeginTask (GettextCatalog.GetString ("Updating"), 5);
			
			// Fetch remote commits
			string remote = GetCurrentRemote ();
			if (remote == null)
				throw new InvalidOperationException ("No remotes defined");
			monitor.Log.WriteLine (GettextCatalog.GetString ("Fetching from '{0}'", remote));
			RemoteConfig remoteConfig = new RemoteConfig (repo.GetConfig (), remote);
			Transport tn = Transport.Open (repo, remoteConfig);
			using (var gm = new GitMonitor (monitor))
				tn.Fetch (gm, null);
			monitor.Step (1);
			
			string upstreamRef = GitUtil.GetUpstreamSource (repo, GetCurrentBranch ());
			if (upstreamRef == null)
				upstreamRef = GetCurrentRemote () + "/" + GetCurrentBranch ();
			
			if (GitService.UseRebaseOptionWhenPulling)
				Rebase (upstreamRef, GitService.StashUnstashWhenUpdating, monitor);
			else
				Merge (upstreamRef, GitService.StashUnstashWhenUpdating, monitor);
			
			monitor.Step (1);
			
			// Notify changes
			if (statusList != null)
				NotifyFileChanges (monitor, statusList);
			
			monitor.EndTask ();
			*/
		}
		
		public void Rebase (string upstreamRef, bool saveLocalChanges, IProgressMonitor monitor)
		{
			throw new NotImplementedException ();
			/*
			ShelfCollection stashes = GitUtil.GetStashes (repo);
			Shelf stash = null;
			
			try
			{
				if (saveLocalChanges) {
					monitor.BeginTask (GettextCatalog.GetString ("Rebasing"), 3);
					monitor.Log.WriteLine (GettextCatalog.GetString ("Saving local changes"));
					using (var gm = new GitMonitor (monitor))
						stash = stashes.Create (gm, GetShelfName ("_tmp_"));
					monitor.Step (1);
				}
				
				NGit.Api.Git git = new NGit.Api.Git (repo);
				RebaseCommand rebase = git.Rebase ();
				rebase.SetOperation (RebaseCommand.Operation.BEGIN);
				rebase.SetUpstream (upstreamRef);
				
				var gmonitor = new GitMonitor (monitor);
				rebase.SetProgressMonitor (gmonitor);
				
				bool aborted = false;
				
				try {
					var result = rebase.Call ();
					while (!aborted && result.GetStatus () == RebaseResult.Status.STOPPED) {
						rebase = git.Rebase ();
						rebase.SetProgressMonitor (gmonitor);
						rebase.SetOperation (RebaseCommand.Operation.CONTINUE);
						bool commitChanges = true;
						var conflicts = GitUtil.GetConflictedFiles (repo);
						foreach (string conflictFile in conflicts) {
							ConflictResult res = ResolveConflict (FromMercurialPath (conflictFile));
							if (res == ConflictResult.Abort) {
								aborted = true;
								commitChanges = false;
								rebase.SetOperation (RebaseCommand.Operation.ABORT);
								break;
							} else if (res == ConflictResult.Skip) {
								rebase.SetOperation (RebaseCommand.Operation.SKIP);
								commitChanges = false;
								break;
							}
						}
						if (commitChanges) {
							NGit.Api.AddCommand cmd = git.Add ();
							foreach (string conflictFile in conflicts)
								cmd.AddFilepattern (conflictFile);
							cmd.Call ();
						}
						result = rebase.Call ();
					}
				} catch {
					if (!aborted) {
						rebase = git.Rebase ();
						rebase.SetOperation (RebaseCommand.Operation.ABORT);
						rebase.SetProgressMonitor (gmonitor);
						rebase.Call ();
					}
					throw;
				} finally {
					gmonitor.Dispose ();
				}
				
			} finally {
				if (saveLocalChanges)
					monitor.Step (1);
				
				// Restore local changes
				if (stash != null) {
					monitor.Log.WriteLine (GettextCatalog.GetString ("Restoring local changes"));
					using (var gm = new GitMonitor (monitor))
						stash.Apply (gm);
					stashes.Remove (stash);
					monitor.EndTask ();
				}
			}		
			*/
		}
		
		public void Merge (string branch, bool saveLocalChanges, IProgressMonitor monitor)
		{
			throw new NotImplementedException ();
			/*
			IEnumerable<DiffEntry> statusList = null;
			Shelf stash = null;
			ShelfCollection stashes = new ShelfCollection (repo);
			monitor.BeginTask (null, 4);
			
			try {
				// Get a list of files that are different in the target branch
				statusList = GitUtil.GetChangedFiles (repo, branch);
				monitor.Step (1);
				
				if (saveLocalChanges) {
					monitor.BeginTask (GettextCatalog.GetString ("Merging"), 3);
					monitor.Log.WriteLine (GettextCatalog.GetString ("Saving local changes"));
					using (var gm = new GitMonitor (monitor))
						stash = stashes.Create (gm, GetShelfName ("_tmp_"));
					monitor.Step (1);
				}
				
				// Apply changes
				
				ObjectId branchId = repo.Resolve (branch);
				
				NGit.Api.Git git = new NGit.Api.Git (repo);
				MergeCommandResult mergeResult = git.Merge ().SetStrategy (MergeStrategy.RESOLVE).Include (branchId).Call ();
				if (mergeResult.GetMergeStatus () == MergeStatus.CONFLICTING || mergeResult.GetMergeStatus () == MergeStatus.FAILED) {
					var conflicts = mergeResult.GetConflicts ();
					bool commit = true;
					if (conflicts != null) {
						foreach (string conflictFile in conflicts.Keys) {
							ConflictResult res = ResolveConflict (FromMercurialPath (conflictFile));
							if (res == ConflictResult.Abort) {
								GitUtil.HardReset (repo, GetHeadCommit ());
								commit = false;
								break;
							} else if (res == ConflictResult.Skip) {
								Revert (FromMercurialPath (conflictFile), false, monitor);
								break;
							}
						}
					}
					if (commit)
						git.Commit ().Call ();
				}
				
			} finally {
				if (saveLocalChanges)
					monitor.Step (1);
				
				// Restore local changes
				if (stash != null) {
					monitor.Log.WriteLine (GettextCatalog.GetString ("Restoring local changes"));
					using (var gm = new GitMonitor (monitor))
						stash.Apply (gm);
					stashes.Remove (stash);
					monitor.EndTask ();
				}
			}
			monitor.Step (1);
			
			// Notify changes
			if (statusList != null)
				NotifyFileChanges (monitor, statusList);
			
			monitor.EndTask ();
			*/
		}
		
		ConflictResult ResolveConflict (string file)
		{
			ConflictResult res = ConflictResult.Abort;
			DispatchService.GuiSyncDispatch (delegate {
				ConflictResolutionDialog dlg = new ConflictResolutionDialog ();
				dlg.Load (file);
				try {
					dlg.Load (file);
					var dres = (Gtk.ResponseType) MessageService.RunCustomDialog (dlg);
					dlg.Hide ();
					switch (dres) {
					case Gtk.ResponseType.Cancel:
						res = ConflictResult.Abort;
						break;
					case Gtk.ResponseType.Close:
						res = ConflictResult.Skip;
						break;
					case Gtk.ResponseType.Ok:
						res = ConflictResult.Continue;
						dlg.Save (file);
						break;
					}
				} finally {
					dlg.Destroy ();
				}
			});
			return res;
		}
		
		public override void Commit (ChangeSet changeSet, IProgressMonitor monitor)
		{
			string message = changeSet.GlobalComment;
			if (string.IsNullOrEmpty (message))
				throw new ArgumentException ("Commit message must not be null or empty!", "message");

			var cmd = new global::Mercurial.CommitCommand ().WithMessage (message);
			if (changeSet.ExtendedProperties.Contains ("Mercurial.AuthorName")) {
				var author = changeSet.ExtendedProperties ["Mercurial.AuthorName"];
				var email = changeSet.ExtendedProperties ["Mercurial.AuthorEmail"];
				cmd = cmd.WithOverrideAuthor (string.Concat ("{0} <{1}>", author, email));
			}
			foreach (var file in changeSet.Items)
				cmd.WithAdditionalArgument (file.LocalPath);
			repo.Commit (cmd);
		}
		
		IEnumerable<string> GetDirectoryFiles (DirectoryInfo dir)
		{
			// mercurial-FIXME: consider ignore flags.
			foreach (var fi in dir.GetFiles ())
				yield return fi.FullName;
			/*
			FileTreeIterator iter = new FileTreeIterator (dir.FullName, repo.FileSystem, WorkingTreeOptions.KEY.Parse(repo.GetConfig()));
			while (!iter.Eof) {
				var file = iter.GetEntryFile ();
				if (file != null && !iter.IsEntryIgnored ())
					yield return file.GetPath ();
				iter.Next (1);
			}
			*/
		}

		public void GetUserInfo (out string name, out string email)
		{
			throw new NotImplementedException ();
			/*
			UserConfig config = repo.GetConfig ().Get (UserConfig.KEY);
			name = config.GetCommitterName ();
			email = config.GetCommitterEmail ();
			*/
		}
		
		public void SetUserInfo (string name, string email)
		{
			throw new NotImplementedException ();
			/*
			NGit.StoredConfig config = repo.GetConfig ();
			config.SetString ("user", null, "name", name);
			config.SetString ("user", null, "email", email);
			config.Save ();
			*/
		}

		// Mercurial-FIXME: use IProgressMonitor.
		public override void Checkout (FilePath targetLocalPath, Revision rev, bool recurse, IProgressMonitor monitor)
		{
			var uri = new Uri (Url);
			var name = Path.GetFileName (uri.LocalPath);
			var path = Path.Combine (targetLocalPath, name);
			if (Directory.Exists (path) || File.Exists (path))
				throw new InvalidOperationException (string.Format ("Cannot create directory '{0}' under {1}: file already exists", name, targetLocalPath));
			Directory.CreateDirectory (path);
			var cmd = new CloneCommand ();
			if (rev != null)
				cmd.WithRevision (new RevSpec (((MercurialRevision) rev).ShortName));
			repo = new global::Mercurial.Repository (path);
			this.path = repo.Path;
			repo.Clone (Url, cmd);
		}
		
		public override void Revert (FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
		{
			// Mercurial-FIXME: use IProgressMonitor
			foreach (var path in localPaths)
				repo.Revert (path);
			/*
			var c = GetHeadCommit ();
			RevTree tree = c != null ? c.Tree : null;
			
			List<FilePath> changedFiles = new List<FilePath> ();
			List<FilePath> removedFiles = new List<FilePath> ();
			
			monitor.BeginTask (GettextCatalog.GetString ("Reverting files"), 3);
			monitor.BeginStepTask (GettextCatalog.GetString ("Reverting files"), localPaths.Length, 2);
			
			DirCache dc = repo.LockDirCache ();
			DirCacheBuilder builder = dc.Builder ();
			
			try {
				HashSet<string> entriesToRemove = new HashSet<string> ();
				HashSet<string> foldersToRemove = new HashSet<string> ();
				
				// Add the new entries
				foreach (FilePath fp in localPaths) {
					string p = ToMercurialPath (fp);
					
					// Register entries to be removed from the index
					if (Directory.Exists (fp))
						foldersToRemove.Add (p);
					else
						entriesToRemove.Add (p);
					
					TreeWalk tw = tree != null ? TreeWalk.ForPath (repo, p, tree) : null;
					if (tw == null) {
						// Removed from the index
					}
					else {
						// Add new entries
						
						TreeWalk r;
						if (tw.IsSubtree) {
							// It's a directory. Make sure we remove existing index entries of this directory
							foldersToRemove.Add (p);
							
							// We have to iterate through all folder files. We need a new iterator since the
							// existing rw is not recursive
							r = new NGit.Treewalk.TreeWalk(repo);
							r.Reset (tree);
							r.Filter = PathFilterGroup.CreateFromStrings(new string[]{p});
							r.Recursive = true;
							r.Next ();
						} else {
							r = tw;
						}
						
						do {
							// There can be more than one entry if reverting a whole directory
							string rpath = FromMercurialPath (r.PathString);
							DirCacheEntry e = new DirCacheEntry (r.PathString);
							e.SetObjectId (r.GetObjectId (0));
							e.FileMode = r.GetFileMode (0);
							if (!Directory.Exists (Path.GetDirectoryName (rpath)))
								Directory.CreateDirectory (rpath);
							DirCacheCheckout.CheckoutEntry (repo, rpath, e);
							builder.Add (e);
							changedFiles.Add (rpath);
						} while (r.Next ());
					}
					monitor.Step (1);
				}
				
				// Add entries we want to keep
				int count = dc.GetEntryCount ();
				for (int n=0; n<count; n++) {
					DirCacheEntry e = dc.GetEntry (n);
					string path = e.PathString;
					if (!entriesToRemove.Contains (path) && !foldersToRemove.Any (f => IsSubpath (f,path)))
						builder.Add (e);
				}
				
				builder.Commit ();
			}
			catch {
				dc.Unlock ();
				throw;
			}
			
			monitor.EndTask ();
			monitor.BeginTask (null, localPaths.Length);
			
			foreach (FilePath p in changedFiles) {
				FileService.NotifyFileChanged (p);
				monitor.Step (1);
			}
			foreach (FilePath p in removedFiles) {
				FileService.NotifyFileRemoved (p);
				monitor.Step (1);
			}
			monitor.EndTask ();
			*/
		}
		
		bool IsSubpath (string basePath, string childPath)
		{
			if (basePath [basePath.Length - 1] == '/')
				return childPath.StartsWith (basePath);
			return childPath.StartsWith (basePath + "/");
		}
		
		// NIE in git too...?
		public override void RevertRevision (FilePath localPath, Revision revision, IProgressMonitor monitor)
		{
			throw new System.NotImplementedException ();
		}
		
		
		// NIE in git too...?
		public override void RevertToRevision (FilePath localPath, Revision revision, IProgressMonitor monitor)
		{
			throw new System.NotImplementedException ();
		}
		
		
		public override void Add (FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
		{
			// mercurial-FIXME: use monitor.
			repo.Add (new global::Mercurial.AddCommand ().WithPaths (localPaths.Select (lp => ToMercurialPath (lp)).ToArray ()).WithRecurseSubRepositories (recurse));
		}
		
		public override void DeleteFiles (FilePath[] localPaths, bool force, IProgressMonitor monitor)
		{
			// mercurial-FIXME: use monitor.
			foreach (var lp in localPaths) {
				repo.Remove (ToMercurialPath (lp), new global::Mercurial.RemoveCommand ().WithForceRemoval (force));
				// Untracked files are not deleted by the rm command, so delete them now
				if (File.Exists (lp))
					File.Delete (lp);
			}
		}
		
		public override void DeleteDirectories (FilePath[] localPaths, bool force, IProgressMonitor monitor)
		{
			// mercurial-FIXME: use monitor.
			foreach (var lp in localPaths) {
				repo.Remove (ToMercurialPath (lp), new global::Mercurial.RemoveCommand ().WithForceRemoval (force));
				// Untracked files are not deleted by the rm command, so delete them now
				if (Directory.Exists (lp))
					Directory.Delete (lp);
			}
		}
		
		public override string GetTextAtRevision (FilePath repositoryPath, Revision revision)
		{
			string path = ToMercurialPath (repositoryPath);
			var rev = ((MercurialRevision) revision).ShortName;
			var ret = repo.Cat (new CatCommand ().WithFile (path).WithRevision (rev)) ?? string.Empty;
			return ret;
		}
		
		public override DiffInfo GenerateDiff (FilePath baseLocalPath, VersionInfo vi)
		{
			var rev = new RevSpec (((MercurialRevision) vi.Revision).ShortName);
			string diff = repo.Diff (new global::Mercurial.DiffCommand ().WithIncludePattern (vi.LocalPath).WithRevisions (rev));
			return new DiffInfo (baseLocalPath, vi.LocalPath, diff);
			/*
			try {
				if ((vi.Status & VersionStatus.ScheduledAdd) != 0) {
					var ctxt = GetFileContent (vi.LocalPath);
					return new DiffInfo (baseLocalPath, vi.LocalPath, GenerateDiff (null, ctxt));
				} else if ((vi.Status & VersionStatus.ScheduledDelete) != 0) {
					var ctxt = GetCommitContent (GetHeadCommit (), vi.LocalPath);
					return new DiffInfo (baseLocalPath, vi.LocalPath, GenerateDiff (ctxt, null));
				} else if ((vi.Status & VersionStatus.Modified) != 0 || (vi.Status & VersionStatus.Conflicted) != 0) {
					var ctxt1 = GetCommitContent (GetHeadCommit (), vi.LocalPath);
					var ctxt2 = GetFileContent (vi.LocalPath);
					return new DiffInfo (baseLocalPath, vi.LocalPath, GenerateDiff (ctxt1, ctxt2));
				}
			} catch (Exception ex) {
				LoggingService.LogError ("Could not get diff for file '" + vi.LocalPath + "'", ex);
			}
			return null;	
			*/
		}
		
		public override DiffInfo[] PathDiff (FilePath baseLocalPath, FilePath[] localPaths, bool remoteDiff)
		{
			List<DiffInfo> diffs = new List<DiffInfo> ();
			VersionInfo[] vinfos = GetDirectoryVersionInfo (baseLocalPath, localPaths, false, true);
			foreach (VersionInfo vi in vinfos) {
				var diff = GenerateDiff (baseLocalPath, vi);
				if (diff != null)
					diffs.Add (diff);
			}
			return diffs.ToArray ();
		}

		/*
		static readonly Encoding utf8_throw_error = new UTF8Encoding (false, true);

		class DiffData
		{
			public DiffData () // no text
			{
			}

			public DiffData (string diff)
			{
				Diff = diff;
			}

			public string Diff { get; set; }
		}

		DiffData GetFileContent (string file)
		{
			try {
				return new DiffData (File.ReadAllText (file, utf8_throw_error));
			} catch (EncoderFallbackException) {
				return new DiffData (); // binary
			}
		}
		
		DiffData GetCommitContent (Changeset c, FilePath file)
		{
			var filePath = ToMercurialPath (file);
			return new DiffData (repo.Diff (new global::Mercurial.DiffCommand ().WithNames (filePath).WithRevisions (c.Revision)));
		}
		
		string GetCommitTextContent (Changeset c, FilePath file)
		{
			return Mono.TextEditor.Utils.TextFileUtility.GetText (GetCommitContent (c, file).Diff);
		}

		string GenerateDiff (DiffData data1, DiffData data2)
		{
			if (data1 != null && data1.Diff == null || data2 != null && data2.Diff == null) {
				if (data1.Length != data2.Length)
					return GettextCatalog.GetString (" Binary files differ");
				if (data1.Length == data2.Length) {
					for (int n=0; n<data1.Length; n++) {
						if (data1[n] != data2[n])
							return GettextCatalog.GetString (" Binary files differ");
					}
				}
				return string.Empty;
			}
			var text1 = new RawText (data1);
			var text2 = new RawText (data2);
			var edits = MyersDiff<RawText>.INSTANCE.Diff(RawTextComparator.DEFAULT, text1, text2);
			MemoryStream s = new MemoryStream ();
			var formatter = new NGit.Diff.DiffFormatter (s);
			formatter.Format (edits, text1, text2);
			return Encoding.UTF8.GetString (s.ToArray ());
		}

		DiffInfo[] GetUnifiedDiffInfo (string diffContent, FilePath basePath, FilePath[] localPaths)
		{
			basePath = basePath.FullPath;
			List<DiffInfo> list = new List<DiffInfo> ();
			using (StringReader sr = new StringReader (diffContent)) {
				string line;
				StringBuilder content = new StringBuilder ();
				string fileName = null;
				
				while ((line = sr.ReadLine ()) != null) {
					if (line.StartsWith ("+++ ") || line.StartsWith ("--- ")) {
						string newFile = path.Combine (line.Substring (6));
						if (fileName != null && fileName != newFile) {
							list.Add (new DiffInfo (basePath, fileName, content.ToString ().Trim ('\n')));
							content = new StringBuilder ();
						}
						fileName = newFile;
					} else if (!line.StartsWith ("diff") && !line.StartsWith ("index")) {
						content.Append (line).Append ('\n');
					}
				}
				if (fileName != null) {
					list.Add (new DiffInfo (basePath, fileName, content.ToString ().Trim ('\n')));
				}
			}
			return list.ToArray ();
		}
		 */

		public string GetCurrentRemote ()
		{
			List<string> remotes = new List<string> (GetRemotes ().Select (r => r.Name));
			if (remotes.Count == 0)
				return null;
			
			if (remotes.Contains ("origin"))
				return "origin";
			else
				return remotes[0];
		}
		
		public void Push (IProgressMonitor monitor, string remote, string remoteBranch)
		{
			throw new NotImplementedException ();
			/*
			RemoteConfig remoteConfig = new RemoteConfig (repo.GetConfig (), remote);
			Transport tp = Transport.Open (repo, remoteConfig);
			
			string remoteRef = "refs/heads/" + remoteBranch;
			
			RemoteRefUpdate rr = new RemoteRefUpdate (repo, repo.GetBranch (), remoteRef, false, null, null);
			List<RemoteRefUpdate> list = new List<RemoteRefUpdate> ();
			list.Add (rr);
			using (var gm = new GitMonitor (monitor))
				tp.Push (gm, list);
			switch (rr.GetStatus ()) {
			case RemoteRefUpdate.Status.UP_TO_DATE: monitor.ReportSuccess (GettextCatalog.GetString ("Remote branch is up to date.")); break;
			case RemoteRefUpdate.Status.REJECTED_NODELETE: monitor.ReportError (GettextCatalog.GetString ("The server is configured to deny deletion of the branch"), null); break;
			case RemoteRefUpdate.Status.REJECTED_NONFASTFORWARD: monitor.ReportError (GettextCatalog.GetString ("The update is a non-fast-forward update. Merge the remote changes before pushing again."), null); break;
			case RemoteRefUpdate.Status.OK:
				monitor.ReportSuccess (GettextCatalog.GetString ("Push operation successfully completed."));
				// Update the remote branch
				ObjectId headId = rr.GetNewObjectId ();
				RefUpdate updateRef = repo.UpdateRef (Constants.R_REMOTES + remote + "/" + remoteBranch);
				updateRef.SetNewObjectId(headId);
				updateRef.Update();
				break;
			default:
				string msg = rr.GetMessage ();
				msg = !string.IsNullOrEmpty (msg) ? msg : GettextCatalog.GetString ("Push operation failed");
				monitor.ReportError (msg, null);
				break;
			}
			*/
		}
		
		public void CreateBranch (string name, string trackSource)
		{
			// In Mercurial there is no way to specify trackSource (disabled)
			repo.Branch (name);
		}
		
		public void SetBranchTrackSource (string name, string trackSource)
		{
			throw new NotSupportedException ();
			//GitUtil.SetUpstreamSource (repo, name, trackSource);
		}
		
		public void RemoveBranch (string name)
		{
			// http://mercurial.selenic.com/wiki/PruningDeadBranches

			// fo safety, we don't try to remove branch which involves cleanup.
			if (repo.Status ().Any (st => st.State != FileState.Clean))
				throw new InvalidOperationException ("We cannot remove branch when there is uncommited changes.");
			var currentBranch = repo.Branch ();
			repo.Update (new global::Mercurial.UpdateCommand ().WithClean ().WithAdditionalArgument (name));
			repo.Commit ("closing branch " + name, new global::Mercurial.CommitCommand ().WithCloseBranch ());
			repo.Update (new global::Mercurial.UpdateCommand ().WithClean ().WithAdditionalArgument (currentBranch));
			//var git = new NGit.Api.Git (repo);
			//git.BranchDelete ().SetBranchNames (name).SetForce (true).Call ();
		}
		
		public void RenameBranch (string name, string newName)
		{
			throw new NotSupportedException ();
			//var git = new NGit.Api.Git (repo);
			//git.BranchRename ().SetOldName (name).SetNewName (newName).Call ();
		}

		public IEnumerable<RemoteRepositoryPath> GetRemotes ()
		{
			return repo.Paths ();
		}
		
		public bool IsBranchMerged (string branchName)
		{
			throw new NotImplementedException ();
			/*
			// check if a branch is merged into HEAD
			RevWalk walk = new RevWalk(repo);
			RevCommit tip = walk.ParseCommit(repo.Resolve(Constants.HEAD));
			Ref currentRef = repo.GetRef(branchName);
			if (currentRef == null)
				return true;
			RevCommit @base = walk.ParseCommit(repo.Resolve(branchName));
			return walk.IsMergedInto(@base, tip);
			*/
		}

		/*
		public void RenameRemote (string name, string newName)
		{
			StoredConfig cfg = repo.GetConfig ();
			RemoteConfig rem = RemoteConfig.GetAllRemoteConfigs (cfg).FirstOrDefault (r => r.Name == name);
			if (rem != null) {
				rem.Name = newName;
				rem.Update (cfg);
			}
		}
		
		public void AddRemote (RemoteSource remote, bool importTags)
		{
			if (string.IsNullOrEmpty (remote.Name))
				throw new InvalidOperationException ("Name not set");
			
			StoredConfig c = repo.GetConfig ();
			RemoteConfig rc = new RemoteConfig (c, remote.Name);
			c.Save ();
			remote.RepoRemote = rc;
			remote.cfg = c;
			UpdateRemote (remote);
		}
		
		public void UpdateRemote (RemoteSource remote)
		{
			if (string.IsNullOrEmpty (remote.FetchUrl))
				throw new InvalidOperationException ("Fetch url can't be empty");
			
			if (remote.RepoRemote == null)
				throw new InvalidOperationException ("Remote not created");
			
			remote.Update ();
			remote.cfg.Save ();
		}
		
		public void RemoveRemote (string name)
		{
			StoredConfig cfg = repo.GetConfig ();
			RemoteConfig rem = RemoteConfig.GetAllRemoteConfigs (cfg).FirstOrDefault (r => r.Name == name);
			if (rem != null) {
				cfg.UnsetSection ("remote", name);
				cfg.Save ();
			}
		}
		*/
		
		public IEnumerable<Branch> GetBranches ()
		{
			foreach (var branch in repo.Branches ())
				yield return new Branch () { Name = branch.Name };
		}
		
		public IEnumerable<Tag> GetTags ()
		{
			return repo.Tags ();
		}
		
		public IEnumerable<string> GetRemoteBranches (string remoteName)
		{
			// in Mercurial remote branches == local branches.
			return repo.Branches ().Select (b => b.Name);
		}
		
		public string GetCurrentBranch ()
		{
			return repo.Branch ();
		}
		
		public void SwitchToBranch (IProgressMonitor monitor, string branch)
		{
			// mercurial-FIXME: use monitor.
			repo.Branch (branch);

			// mercurial-FIXME: Notify file changes caused by changing the branch.

			/*
			monitor.BeginTask (GettextCatalog.GetString ("Switching to branch {0}", branch), GitService.StashUnstashWhenSwitchingBranches ? 4 : 2);
			
			// Get a list of files that are different in the target branch
			IEnumerable<DiffEntry> statusList = GitUtil.GetChangedFiles (repo, branch);
			
			ShelfCollection stashes = null;
			Shelf stash = null;
			
			if (GitService.StashUnstashWhenSwitchingBranches) {
				stashes = GitUtil.GetStashes (repo);
				
				// Remove the stash for this branch, if exists
				string currentBranch = GetCurrentBranch ();
				stash = GetShelfForBranch (stashes, currentBranch);
				if (stash != null)
					stashes.Remove (stash);
				
				// Create a new stash for the branch. This allows switching branches
				// without losing local changes
				using (var gm = new GitMonitor (monitor))
					stash = stashes.Create (gm, GetShelfName (currentBranch));
				
				monitor.Step (1);
			}
			
			// Switch to the target branch
			DirCache dc = repo.LockDirCache ();
			try {
				RevWalk rw = new RevWalk (repo);
				ObjectId branchHeadId = repo.Resolve (branch);
				if (branchHeadId == null)
					throw new InvalidOperationException ("Branch head commit not found");
				
				RevCommit branchCommit = rw.ParseCommit (branchHeadId);
				DirCacheCheckout checkout = new DirCacheCheckout (repo, null, dc, branchCommit.Tree);
				checkout.Checkout ();
				
				RefUpdate u = repo.UpdateRef(Constants.HEAD);
				u.Link ("refs/heads/" + branch);
				monitor.Step (1);
			} catch {
				dc.Unlock ();
				if (GitService.StashUnstashWhenSwitchingBranches) {
					// If something goes wrong, restore the work tree status
					using (var gm = new GitMonitor (monitor))
						stash.Apply (gm);
					stashes.Remove (stash);
				}
				throw;
			}
			
			// Restore the branch stash
			
			if (GitService.StashUnstashWhenSwitchingBranches) {
				stash = GetShelfForBranch (stashes, branch);
				if (stash != null) {
					using (var gm = new GitMonitor (monitor))
						stash.Apply (gm);
					stashes.Remove (stash);
				}
				monitor.Step (1);
			}
			
			// Notify file changes
			
			NotifyFileChanges (monitor, statusList);
			
			if (BranchSelectionChanged != null)
				BranchSelectionChanged (this, EventArgs.Empty);
			
			monitor.EndTask ();
			*/
		}

		/*
		void NotifyFileChanges (IProgressMonitor monitor, IEnumerable<DiffEntry> statusList)
		{
			List<DiffEntry> changes = new List<DiffEntry> (statusList);
			
			// Files added to source branch not present to target branch.
			var removed = changes.Where (c => c.GetChangeType () == DiffEntry.ChangeType.ADD).Select (c => FromMercurialPath (c.GetNewPath ())).ToList ();
			var modified = changes.Where (c => c.GetChangeType () != DiffEntry.ChangeType.ADD).Select (c => FromMercurialPath (c.GetNewPath ())).ToList ();
			
			monitor.BeginTask (GettextCatalog.GetString ("Updating solution"), removed.Count + modified.Count);
			
			FileService.NotifyFilesChanged (modified);
			monitor.Step (modified.Count);
			
			FileService.NotifyFilesRemoved (removed);
			monitor.Step (removed.Count);
			
			monitor.EndTask ();
		}
		*/
		
		static string GetShelfName (string branchName)
		{
			return "__MD_" + branchName;
		}
		
		public static string GetShelfBranchName (string shelfName)
		{
			if (shelfName.StartsWith ("__MD_"))
				return shelfName.Substring (5);
			else
				return null;
		}
		
		Shelf GetShelfForBranch (ShelfCollection shelves, string branchName)
		{
			string sn = GetShelfName (branchName);
			foreach (Shelf ss in shelves) {
				if (ss.Comment.IndexOf (sn) != -1)
					return ss;
			}
			return null;
		}
		
		public ChangeSet GetPushChangeSet (string remote, string branch)
		{
			ChangeSet cset = CreateChangeSet (path);
			if (repo.Branch () != branch)
				return cset; // hg cannot retrieve diff in different branches; return empty.
			var outs = repo.Outgoing (new OutgoingCommand ().WithBranch (branch).WithDestination (remote));
			if (!outs.Any ())
				return cset; // empty
			var oldest = GetPreviousRevisionFor (new MercurialRevision (this, outs.First ().Hash));
			var youngest = outs.Last ();
			var stats = repo.Status (new StatusCommand ().WithRevisions (oldest.Name + ':' + youngest.Hash));

			foreach (var change in stats) {
				VersionStatus status;
				switch (change.State) {
				case FileState.Added:
					status = VersionStatus.ScheduledAdd;
					break;
				case FileState.Removed:
					status = VersionStatus.ScheduledDelete;
					break;
				default:
					status = VersionStatus.Modified;
					break;
				}
				VersionInfo vi = new VersionInfo (FromMercurialPath (change.Path), "", false, status | VersionStatus.Versioned, null, VersionStatus.Versioned, null);
				cset.AddFile (vi);
			}
			return cset;
		}
		
		FilePath FromMercurialPath (string filePath)
		{
			filePath = filePath.Replace ('/', Path.DirectorySeparatorChar);
			return path.Combine (filePath);
		}
		
		public DiffInfo[] GetPushDiff (string remote, string branch)
		{
			List<DiffInfo> diffs = new List<DiffInfo> ();
			if (repo.Branch () != branch)
				return diffs.ToArray (); // hg cannot retrieve diff in different branches; return empty.
			var outs = repo.Outgoing (new OutgoingCommand ().WithBranch (branch).WithDestination (remote));
			if (!outs.Any ())
				return diffs.ToArray (); // empty
			var oldest = GetPreviousRevisionFor (new MercurialRevision (this, outs.First ().Hash));
			var youngest = outs.Last ();
			string revs = oldest.Name + ':' + youngest.Hash;
			var stats = repo.Status (new StatusCommand ().WithRevisions (revs));

			foreach (var change in stats) {
				var cmd = new global::Mercurial.DiffCommand ().WithRevisions (revs).WithNames (new string [] {change.Path});
				var diff = repo.Diff (cmd);
				diffs.Add (new DiffInfo (path, Path.Combine (path, ToMercurialPath (new FilePath (change.Path))), diff));
			}
			return diffs.ToArray ();
		}
		
		public override void MoveFile (FilePath localSrcPath, FilePath localDestPath, bool force, IProgressMonitor monitor)
		{
			VersionInfo vi = GetVersionInfo (localSrcPath, false);
			if (vi == null || !vi.IsVersioned) {
				base.MoveFile (localSrcPath, localDestPath, force, monitor);
				return;
			}
			base.MoveFile (localSrcPath, localDestPath, force, monitor);
			Add (localDestPath, false, monitor);
			
			if ((vi.Status & VersionStatus.ScheduledAdd) != 0)
				Revert (localSrcPath, false, monitor);
		}
		
		public override void MoveDirectory (FilePath localSrcPath, FilePath localDestPath, bool force, IProgressMonitor monitor)
		{
			VersionInfo[] versionedFiles = GetDirectoryVersionInfo (localSrcPath, false, true);
			base.MoveDirectory (localSrcPath, localDestPath, force, monitor);
			monitor.BeginTask ("Moving files", versionedFiles.Length);
			foreach (VersionInfo vif in versionedFiles) {
				if (vif.IsDirectory)
					continue;
				FilePath newDestPath = vif.LocalPath.ToRelative (localSrcPath).ToAbsolute (localDestPath);
				Add (newDestPath, false, monitor);
				monitor.Step (1);
			}
			monitor.EndTask ();
		}

		// Mercurial-FIXME: not working
		public override Annotation[] GetAnnotations (FilePath repositoryPath)
		{
			List<Annotation> list = new List<Annotation> ();
			var cmd = new AnnotateCommand ().WithPath (ToMercurialPath (repositoryPath)).WithAddUserName (true).WithAddDate (true);
			var anns = repo.Annotate (cmd);
			foreach (var ann in anns.OrderBy (a => a.LineNumber)) {
				//var rev = repo.Log (new RevSpec (ann.RevisionNumber.ToString ())).First ();
				//string name = rev.AuthorName + (rev.AuthorEmailAddress != null ? " <" + rev.AuthorEmailAddress + ">" : null);
				list.Add (new Annotation (ann.RevisionNumber.ToString (), ann.User, ann.Date.Date));
			}
			return list.ToArray ();
		}
		
		internal MercurialRevision GetPreviousRevisionFor (MercurialRevision revision)
		{
			var cmd = new global::Mercurial.LogCommand ().WithRevision (revision.Name + ":0");
			if (revision.FileForChanges != null)
				cmd.WithIncludePattern (ToMercurialPath (revision.FileForChanges));
			var prev = repo.Log (cmd).Skip (1).FirstOrDefault ();
			return prev == null ? null : new MercurialRevision (this, prev.Hash);
		}
	}
	
	public class MercurialRevision: Revision
	{
		string rev;
		
		internal Changeset Commit { get; set; }
		internal FilePath FileForChanges { get; set; }
		
		public MercurialRevision (Repository repo, string rev) : base(repo)
		{
			if (rev.Length < 10 && rev != "") throw new Exception ();
			this.rev = rev;
		}
		
		public MercurialRevision (Repository repo, string rev, DateTime time, string author, string message) : base(repo, time, author, message)
		{
			if (rev.Length < 10 && rev != "") throw new Exception ();
			this.rev = rev;
		}
		
		public override string ToString ()
		{
			return rev;
		}
		
		public override string ShortName {
			get {
				if (rev.Length > 10)
					return rev.Substring (0, 10);
				else
					return rev;
			}
		}
		
		public override Revision GetPrevious ()
		{
			return ((MercurialRepository) this.Repository).GetPreviousRevisionFor (this);
		}
	}
	
	public class Branch
	{
		public string Name { get; internal set; }
		public string Tracking { get; internal set; }
	}

	/*
	public class RemoteSource
	{
		internal RemoteConfig RepoRemote;
		internal StoredConfig cfg;
		
		public RemoteSource ()
		{
		}
		
		internal RemoteSource (StoredConfig cfg, RemoteConfig rem)
		{
			this.cfg = cfg;
			RepoRemote = rem;
			Name = rem.Name;
			FetchUrl = rem.URIs.Select (u => u.ToString ()).FirstOrDefault ();
			PushUrl = rem.PushURIs.Select (u => u.ToString ()).FirstOrDefault ();
			if (string.IsNullOrEmpty (PushUrl))
				PushUrl = FetchUrl;
		}
		
		internal void Update ()
		{
			RepoRemote.Name = Name;
			
			var list = new List<URIish> (RepoRemote.URIs);
			list.ForEach (u => RepoRemote.RemoveURI (u));
			
			list = new List<URIish> (RepoRemote.PushURIs);

list.ForEach (u => RepoRemote.RemovePushURI (u));
			
			RepoRemote.AddURI (new URIish (FetchUrl));
			if (!string.IsNullOrEmpty (PushUrl) && PushUrl != FetchUrl)
				RepoRemote.AddPushURI (new URIish (PushUrl));
			RepoRemote.Update (cfg);
		}
		
		public string Name { get; internal set; }
		public string FetchUrl { get; internal set; }
		public string PushUrl { get; internal set; }
	}
	*/

	/*
	class MercurialMonitor: NGit.ProgressMonitor, IDisposable
	{
		IProgressMonitor monitor;
		int currentWork;
		int currentStep;
		bool taskStarted;
		int totalTasksOverride = -1;
		bool monitorStarted;
		
		public MercurialMonitor (IProgressMonitor monitor)
		{
			this.monitor = monitor;
		}
		
		public MercurialMonitor (IProgressMonitor monitor, int totalTasksOverride)
		{
			this.monitor = monitor;
			this.totalTasksOverride = totalTasksOverride;
		}
		
		public override void Start (int totalTasks)
		{
			monitorStarted = true;
			currentStep = 0;
			currentWork = totalTasksOverride != -1 ? totalTasksOverride : (totalTasks > 0 ? totalTasks : 1);
			totalTasksOverride = -1;
			monitor.BeginTask (null, currentWork);
		}
		
		
		public override void BeginTask (string title, int totalWork)
		{
			if (taskStarted)
				EndTask ();
			
			taskStarted = true;
			currentStep = 0;
			currentWork = totalWork > 0 ? totalWork : 1;
			monitor.BeginTask (title, currentWork);
		}
		
		
		public override void Update (int completed)
		{
			currentStep += completed;
			if (currentStep >= (currentWork / 100)) {
				monitor.Step (currentStep);
				currentStep = 0;
			}
		}
		
		
		public override void EndTask ()
		{
			taskStarted = false;
			monitor.EndTask ();
			monitor.Step (1);
		}
		
		
		public override bool IsCancelled ()
		{
			return monitor.IsCancelRequested;
		}
		
		public void Dispose ()
		{
			if (monitorStarted)
				monitor.EndTask ();
		}
	}

	class LocalMercurialRepository: global::Mercurial.Repository
	{
		WeakReference dirCacheRef;
		DateTime dirCacheTimestamp;
		
		public LocalMercurialRepository (string path): base (path)
		{
		}
		
		public override DirCache ReadDirCache ()
		{
			DirCache dc = null;
			if (dirCacheRef != null)
				dc = dirCacheRef.Target as DirCache;
			if (dc != null) {
				DateTime wt = File.GetLastWriteTime (GetIndexFile ());
				if (wt == dirCacheTimestamp)
					return dc;
			}
			dirCacheTimestamp = File.GetLastWriteTime (GetIndexFile ());
			dc = base.ReadDirCache ();
			dirCacheRef = new WeakReference (dc);
			return dc;
		}
	}
	*/
}

