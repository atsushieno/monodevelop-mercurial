MonoDevelop.VersionControl.Mercurial is a Mercurial addin for monodevelop.

The basic structure is copied from MonoDevelop.VersionControl.Git in
monodevelop source tree.

MonoDevelop.Mercurial uses Mercurial.Net and Mercurial.Net uses 
command line hg client, so it is supposed to be in your PATH.

Currently it is built only with MonoDevelop master.

It is not very solid yet. Be careful to deal with non-referencing
features such as commit and push.

