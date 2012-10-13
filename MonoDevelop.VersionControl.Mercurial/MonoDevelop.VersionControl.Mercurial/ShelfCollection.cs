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

using DiffEntry = System.String;// diffs in Mercurial.NET are mere strings.

namespace MonoDevelop.VersionControl.Mercurial
{
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

