// 
// Shelf.cs
//  
// Author:
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

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Mercurial;

using MergeCommandResult = Mercurial.MergeCommand;
using PersonIdent = System.String; // maybe ...

namespace MonoDevelop.VersionControl.Mercurial
{
	public class Shelf
	{
		internal string CommitId { get; private set; }
		internal string FullLine { get; private set; }
		internal ShelfCollection ShelfCollection { get; set; }
		
		/// <summary>
		/// Who created the stash
		/// </summary>
		public string Author { get; private set; }
		public string AuthorEmail { get; private set; }

		/// <summary>
		/// Timestamp of the stash creation
		/// </summary>
		public DateTimeOffset DateTime { get; private set; }
		
		/// <summary>
		/// Shelf comment
		/// </summary>
		public string Comment { get; private set; }
		
		private Shelf ()
		{
		}
		
		internal Shelf (string prevShelfCommitId, string commitId, string author, string email, string comment)
		{
			this.PrevShelfCommitId = prevShelfCommitId;
			this.CommitId = commitId;
			this.Author = author;
			this.AuthorEmail = email;
			this.DateTime = DateTimeOffset.Now;
			
			// Skip "WIP on master: "
			int i = comment.IndexOf (':');
			this.Comment = comment.Substring (i + 2);			
			
			// Create the text line to be written in the stash log
			
			int secs = (int) (this.DateTime - new DateTimeOffset (1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds;
			
			TimeSpan ofs = this.DateTime.Offset;
			string tz = string.Format ("{0}{1:00}{2:00}", (ofs.Hours >= 0 ? '+':'-'), Math.Abs (ofs.Hours), Math.Abs (ofs.Minutes));
			
			StringBuilder sb = new StringBuilder ();
			sb.Append (prevShelfCommitId ?? new string ('0', 40)).Append (' ');
			sb.Append (commitId).Append (' ');
			sb.Append (author).Append (" <").Append (email).Append ("> ");
			sb.Append (secs).Append (' ').Append (tz).Append ('\t');
			sb.Append (comment);
			FullLine = sb.ToString ();
		}
		
		string prevShelfCommitId;
		
		internal string PrevShelfCommitId {
			get { return prevShelfCommitId; }
			set {
				prevShelfCommitId = value;
				if (FullLine != null) {
					if (prevShelfCommitId != null)
						FullLine = prevShelfCommitId + FullLine.Substring (40);
					else
						FullLine = new string ('0', 40) + FullLine.Substring (40);
				}
			}
		}
		
		
		internal static Shelf Parse (string line)
		{
			// Parses a stash log line and creates a Shelf object with the information
			
			Shelf s = new Shelf ();
			s.PrevShelfCommitId = line.Substring (0, 40);
			if (s.PrevShelfCommitId.All (c => c == '0')) // And id will all 0 means no parent (first stash of the stack)
				s.PrevShelfCommitId = null;
			s.CommitId = line.Substring (41, 40);
			
			string aname = string.Empty;
			string amail = string.Empty;
			
			int i = line.IndexOf ('<');
			if (i != -1) {
				aname = line.Substring (82, i - 82 - 1);
				i++;
				int i2 = line.IndexOf ('>', i);
				if (i2 != -1)
					amail = line.Substring (i, i2 - i);
				
				i2 += 2;
				i = line.IndexOf (' ', i2);
				int secs = int.Parse (line.Substring (i2, i - i2));
				DateTime t = new DateTime (1970, 1, 1) + TimeSpan.FromSeconds (secs);
				string st = t.ToString ("yyyy-MM-ddTHH:mm:ss") + line.Substring (i + 1, 3) + ":" + line.Substring (i + 4, 2);
				s.DateTime = DateTimeOffset.Parse (st);
				s.Comment = line.Substring (i + 7);
				i = s.Comment.IndexOf (':');
				if (i != -1)
					s.Comment = s.Comment.Substring (i + 2);			
			}
			s.Author = aname;
			s.AuthorEmail = amail;
			s.FullLine = line;
			return s;
		}
		
		public global::Mercurial.MergeCommand Apply (MercurialProgressMonitor monitor)
		{
			return ShelfCollection.Apply (monitor, this);
		}
	}
	
	public class ShelfCollection: IEnumerable<Shelf>
	{
		global::Mercurial.Repository _repo;
		
		internal ShelfCollection (global::Mercurial.Repository repo)
		{
			this._repo = repo;
		}
		
		FileInfo ShelfLogFile {
			get {
				string stashLog = Path.Combine (_repo.Client.RepositoryPath, "logs");
				stashLog = Path.Combine (stashLog, "refs");
				return new FileInfo (Path.Combine (stashLog, "stash"));
			}
		}
		
		FileInfo ShelfRefFile {
			get {
				string file = Path.Combine (_repo.Path, "refs");
				return new FileInfo (Path.Combine (file, "stash"));
			}
		}
		
		public Shelf Create (MercurialProgressMonitor monitor)
		{
			return Create (monitor, null);
		}
		
		public Shelf Create (MercurialProgressMonitor monitor, string message)
		{
			throw new NotImplementedException ();
		}
		/*
		public Shelf Create (MercurialProgressMonitor monitor, string message)
		{
			if (monitor != null) {
				monitor.Start (1);
				monitor.BeginTask ("Shelving changes", 100);
			}
			
			UserConfig config = _repo.GetConfig ().Get (UserConfig.KEY);
			RevWalk rw = new RevWalk (_repo);
			ObjectId headId = _repo.Resolve (Constants.HEAD);
			var parent = rw.ParseCommit (headId);
			
			PersonIdent author = new PersonIdent(config.GetAuthorName () ?? "unknown", config.GetAuthorEmail () ?? "unknown@(none).");
			
			if (string.IsNullOrEmpty (message)) {
				// Use the commit summary as message
				message = parent.Abbreviate (7).ToString () + " " + parent.GetShortMessage ();
				int i = message.IndexOfAny (new char[] { '\r', '\n' });
				if (i != -1)
					message = message.Substring (0, i);
			}
			
			// Create the index tree commit
			ObjectInserter inserter = _repo.NewObjectInserter ();
			DirCache dc = _repo.ReadDirCache ();
			
			if (monitor != null)
				monitor.Update (10);
			
			var tree_id = dc.WriteTree (inserter);
			inserter.Release ();
			
			if (monitor != null)
				monitor.Update (10);
			
			string commitMsg = "index on " + _repo.GetBranch () + ": " + message;
			ObjectId indexCommit = GitUtil.CreateCommit (_repo, commitMsg + "\n", new ObjectId[] {headId}, tree_id, author, author);
			
			if (monitor != null)
				monitor.Update (20);
			
			// Create the working dir commit
			tree_id = WriteWorkingDirectoryTree (parent.Tree, dc);
			commitMsg = "WIP on " + _repo.GetBranch () + ": " + message;
			var wipCommit = GitUtil.CreateCommit(_repo, commitMsg + "\n", new ObjectId[] { headId, indexCommit }, tree_id, author, author);
			
			if (monitor != null)
				monitor.Update (20);
			
			string prevCommit = null;
			FileInfo sf = ShelfRefFile;
			if (sf.Exists)
				prevCommit = File.ReadAllText (sf.FullName).Trim (' ','\t','\r','\n');
			
			Shelf s = new Shelf (prevCommit, wipCommit.Name, author, commitMsg);
			
			FileInfo stashLog = ShelfLogFile;
			File.AppendAllText (stashLog.FullName, s.FullLine + "\n");
			File.WriteAllText (sf.FullName, s.CommitId + "\n");
			
			if (monitor != null)
				monitor.Update (5);
			
			// Wipe all local changes
			GitUtil.HardReset (_repo, Constants.HEAD);
			
			monitor.EndTask ();
			s.ShelfCollection = this;
			return s;
		}
		
		ObjectId WriteWorkingDirectoryTree (RevTree headTree, DirCache index)
		{
			DirCache dc = DirCache.NewInCore ();
			DirCacheBuilder cb = dc.Builder ();
			
			ObjectInserter oi = _repo.NewObjectInserter ();
			try {
				TreeWalk tw = new TreeWalk (_repo);
				tw.Reset ();
				tw.AddTree (new FileTreeIterator (_repo));
				tw.AddTree (headTree);
				tw.AddTree (new DirCacheIterator (index));
				
				while (tw.Next ()) {
					// Ignore untracked files
					if (tw.IsSubtree)
						tw.EnterSubtree ();
					else if (tw.GetFileMode (0) != NGit.FileMode.MISSING && (tw.GetFileMode (1) != NGit.FileMode.MISSING || tw.GetFileMode (2) != NGit.FileMode.MISSING)) {
						WorkingTreeIterator f = tw.GetTree<WorkingTreeIterator>(0);
						DirCacheIterator dcIter = tw.GetTree<DirCacheIterator>(2);
						DirCacheEntry currentEntry = dcIter.GetDirCacheEntry ();
						DirCacheEntry ce = new DirCacheEntry (tw.PathString);
						if (!f.IsModified (currentEntry, true)) {
							ce.SetLength (currentEntry.Length);
							ce.LastModified = currentEntry.LastModified;
							ce.FileMode = currentEntry.FileMode;
							ce.SetObjectId (currentEntry.GetObjectId ());
						}
						else {
							long sz = f.GetEntryLength();
							ce.SetLength (sz);
							ce.LastModified = f.GetEntryLastModified();
							ce.FileMode = f.EntryFileMode;
							var data = f.OpenEntryStream();
							try {
								ce.SetObjectId (oi.Insert (Constants.OBJ_BLOB, sz, data));
							} finally {
								data.Close ();
							}
						}
						cb.Add (ce);
					}
				}
				
				cb.Finish ();
				return dc.WriteTree (oi);
			} finally {
				oi.Release ();
			}
		}
		*/
		
		internal MergeCommandResult Apply (MercurialProgressMonitor monitor, Shelf shelf)
		{
			/*
			monitor.Start (1);
			monitor.BeginTask ("Applying stash", 100);
			ObjectId cid = _repo.Resolve (shelf.CommitId);
			RevWalk rw = new RevWalk (_repo);
			RevCommit wip = rw.ParseCommit (cid);
			RevCommit oldHead = wip.Parents.First();
			rw.ParseHeaders (oldHead);
			MergeCommandResult res = GitUtil.MergeTrees (monitor, _repo, oldHead, wip, "Shelf", false);
			monitor.EndTask ();
			return res;
			*/
			throw new NotImplementedException ();
		}
		
		public void Remove (Shelf s)
		{
			List<Shelf> stashes = ReadShelfes ();
			Remove (stashes, s);
		}
		
		public MergeCommandResult Pop (MercurialProgressMonitor monitor)
		{
			List<Shelf> stashes = ReadShelfes ();
			Shelf last = stashes.Last ();
			MergeCommandResult res = last.Apply (monitor);
			if (res.Result == MergeResult.Success)
				Remove (stashes, last);
			return res;
		}
		
		public void Clear ()
		{
			if (ShelfRefFile.Exists)
				ShelfRefFile.Delete ();
			if (ShelfLogFile.Exists)
				ShelfLogFile.Delete ();
		}
		
		void Remove (List<Shelf> stashes, Shelf s)
		{
			int i = stashes.FindIndex (st => st.CommitId == s.CommitId);
			if (i != -1) {
				stashes.RemoveAt (i);
				Shelf next = stashes.FirstOrDefault (ns => ns.PrevShelfCommitId == s.CommitId);
				if (next != null)
					next.PrevShelfCommitId = s.PrevShelfCommitId;
				if (stashes.Count == 0) {
					// No more stashes. The ref and log files can be deleted.
					ShelfRefFile.Delete ();
					ShelfLogFile.Delete ();
					return;
				}
				WriteShelfes (stashes);
				if (i == stashes.Count) {
					// We deleted the head. Write the new head.
					File.WriteAllText (ShelfRefFile.FullName, stashes.Last ().CommitId + "\n");
				}
			}
		}
		
		public IEnumerator<Shelf> GetEnumerator ()
		{
			return ReadShelfes ().GetEnumerator ();
		}
		
		List<Shelf> ReadShelfes ()
		{
			// Reads the registered stashes
			// Results are returned from the bottom to the top of the stack
			
			List<Shelf> result = new List<Shelf> ();
			FileInfo logFile = ShelfLogFile;
			if (!logFile.Exists)
				return result;
			
			Dictionary<string,Shelf> stashes = new Dictionary<string, Shelf> ();
			Shelf first = null;
			foreach (string line in File.ReadAllLines (logFile.FullName)) {
				Shelf s = Shelf.Parse (line);
				s.ShelfCollection = this;
				if (s.PrevShelfCommitId == null)
					first = s;
				else
					stashes.Add (s.PrevShelfCommitId, s);
			}
			while (first != null) {
				result.Add (first);
				stashes.TryGetValue (first.CommitId, out first);
			}
			return result;
		}
		
		void WriteShelfes (List<Shelf> list)
		{
			StringBuilder sb = new StringBuilder ();
			foreach (var s in list) {
				sb.Append (s.FullLine);
				sb.Append ('\n');
			}
			File.WriteAllText (ShelfLogFile.FullName, sb.ToString ());
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}
