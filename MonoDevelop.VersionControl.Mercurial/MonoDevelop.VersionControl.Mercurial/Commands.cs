// 
// Command.cs
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
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.Core;
using System.Linq;
using MonoDevelop.Ide.ProgressMonitoring;
using System.Threading;

namespace MonoDevelop.VersionControl.Mercurial
{
	public enum Commands
	{
		Push,
		SwitchToBranch,
		//ManageBranches,
		Merge,
		Rebase,
#if ShelveSupported // can't be enabled unless Mercurial.net supports ShelveExtension.
		Stash,
		StashPop,
		ManageStashes
#endif
	}
	
	class MercurialCommandHandler: CommandHandler
	{
		public MercurialRepository Repository {
			get {
				IWorkspaceObject wob = IdeApp.ProjectOperations.CurrentSelectedSolutionItem;
				if (wob == null)
					wob = IdeApp.ProjectOperations.CurrentSelectedWorkspaceItem;
				if (wob != null)
					return VersionControlService.GetRepository (wob) as MercurialRepository;
				else
					return null;
			}
		}
		
		protected override void Update (CommandInfo info)
		{
			info.Visible = Repository != null;
		}
	}
	
	class PushCommandHandler: MercurialCommandHandler
	{
		protected override void Run ()
		{
			MercurialService.Push (Repository);
		}
	}
	
	class SwitchToBranchHandler: MercurialCommandHandler
	{
		protected override void Run (object dataItem)
		{
			MercurialService.SwitchToBranch (Repository, (string) dataItem);
		}
		
		protected override void Update (CommandArrayInfo info)
		{
			MercurialRepository repo = Repository;
			if (repo != null) {
				string currentBranch = repo.GetCurrentBranch ();
				foreach (var branch in repo.GetBranches ()) {
					CommandInfo ci = info.Add (branch.Name, branch.Name);
					if (branch.Name == currentBranch)
						ci.Checked = true;
				}
			}
		}
	}

	class ManageBranchesHandler: MercurialCommandHandler
	{
		protected override void Run ()
		{
			MercurialService.ShowConfigurationDialog (Repository);
		}
	}
	
	class MergeBranchHandler: MercurialCommandHandler
	{
		protected override void Run ()
		{
			MercurialService.ShowMergeDialog (Repository, false);
		}
	}
	
	class RebaseBranchHandler: MercurialCommandHandler
	{
		protected override void Run ()
		{
			MercurialService.ShowMergeDialog (Repository, true);
		}
	}

#if ShelveSupported // can't be enabled unless Mercurial.net supports ShelveExtension.
	class StashHandler: MercurialCommandHandler
	{
		protected override void Run ()
		{
			throw new NotImplementedException ();
			/*
			var stashes = Repository.GetStashes ();
			NewStashDialog dlg = new NewStashDialog ();
			if (MessageService.RunCustomDialog (dlg) == (int) Gtk.ResponseType.Ok) {
				string comment = dlg.Comment;
				MessageDialogProgressMonitor monitor = new MessageDialogProgressMonitor (true, false, false, true);
				var statusTracker = IdeApp.Workspace.GetFileStatusTracker ();
				ThreadPool.QueueUserWorkItem (delegate {
					try {
						using (var gm = new GitMonitor (monitor))
							stashes.Create (gm, comment);
					} catch (Exception ex) {
						MessageService.ShowException (ex);
					}
					finally {
						monitor.Dispose ();
						statusTracker.NotifyChanges ();
					}
				});
			}
			dlg.Destroy ();
			*/
		}
	}
	
	class StashPopHandler: MercurialCommandHandler
	{
		protected override void Run ()
		{
			throw new NotImplementedException ();
			/*
			var stashes = Repository.GetStashes ();
			MessageDialogProgressMonitor monitor = new MessageDialogProgressMonitor (true, false, false, true);
			var statusTracker = IdeApp.Workspace.GetFileStatusTracker ();
			ThreadPool.QueueUserWorkItem (delegate {
				try {
					NGit.Api.MergeCommandResult result;
					using (var gm = new GitMonitor (monitor))
						result = stashes.Pop (gm);
					GitService.ReportStashResult (monitor, result);
				} catch (Exception ex) {
					MessageService.ShowException (ex);
				}
				finally {
					monitor.Dispose ();
					statusTracker.NotifyChanges ();
				}
			});
			*/
		}
		
		protected override void Update (CommandInfo info)
		{
			throw new NotImplementedException ();
			/*
			var repo = Repository;
			if (repo != null) {
				var s = repo.GetStashes ();
				info.Enabled = s.Any ();
			} else
				info.Visible = false;
			*/
		}
	}
	
	class ManageStashesHandler: MercurialCommandHandler
	{
		protected override void Run ()
		{
			MercurialService.ShowShelfManager (Repository);
		}
	}
#endif
}

