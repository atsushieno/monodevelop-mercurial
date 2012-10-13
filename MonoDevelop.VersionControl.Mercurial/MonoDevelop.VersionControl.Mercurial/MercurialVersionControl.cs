// 
// MercurialVersionControl.cs
//  
// Author:
//       Atsushi Eno <atsushi@xamarin.com>
//       Dale Ragan <dale.ragan@sinesignal.com>
// 
// Copyright (c) 2012 Xamarin Inc.
// Copyright (c) 2010 SineSignal, LLC
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
using MonoDevelop.Core;
using System.IO;
using System.Collections.Generic;

namespace MonoDevelop.VersionControl.Mercurial
{
	public abstract class MercurialVersionControl : VersionControlSystem
	{
		Dictionary<FilePath,MercurialRepository> repositories = new Dictionary<FilePath,MercurialRepository> ();
		
		static MercurialVersionControl ()
		{
			// mercurial-FIXME: do we need default credential like git support does?
		}
		
		public override string Name {
			get { return "Mercurial"; }
		}
		
		public override bool IsInstalled {
			get {
				return true;
			}
		}
		
		public override Repository GetRepositoryReference (FilePath path, string id)
		{
			if (path.IsEmpty || path.ParentDirectory.IsEmpty || path.IsNull || path.ParentDirectory.IsNull)
				return null;
			if (System.IO.Directory.Exists (path.Combine (".hg"))) {
				MercurialRepository repo;
				if (!repositories.TryGetValue (path.CanonicalPath, out repo))
					repositories [path.CanonicalPath] = repo = new MercurialRepository (path, null);
				return repo;
			}
			else
				return GetRepositoryReference (path.ParentDirectory, id);
		}
		
		protected override Repository OnCreateRepositoryInstance ()
		{
			return new MercurialRepository ();
		}
		
		public override IRepositoryEditor CreateRepositoryEditor (Repository repo)
		{
			return new UrlBasedRepositoryEditor ((MercurialRepository)repo);
		}
		
		internal void UnregisterRepo (MercurialRepository repo)
		{
			if (!repo.RootPath.IsNullOrEmpty)
				repositories.Remove (repo.RootPath.CanonicalPath);
		}
	}
}

