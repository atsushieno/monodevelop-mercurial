
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.VersionControl.Mercurial
{
	public partial class ShelfManagerDialog
	{
		private global::Gtk.HBox hbox2;
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		private global::Gtk.TreeView list;
		private global::Gtk.VBox vboxButtons;
		private global::Gtk.Button buttonApplyRemove;
		private global::Gtk.Button buttonApply;
		private global::Gtk.Button buttonBranch;
		private global::Gtk.HSeparator hseparator1;
		private global::Gtk.Button buttonDelete;
		private global::Gtk.Button buttonOk;
		
		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MonoDevelop.VersionControl.Mercurial.ShelfManagerDialog
			this.Name = "MonoDevelop.VersionControl.Mercurial.ShelfManagerDialog";
			this.Title = global::Mono.Unix.Catalog.GetString ("Shelf Manager");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child MonoDevelop.VersionControl.Mercurial.ShelfManagerDialog.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			this.hbox2.BorderWidth = ((uint)(9));
			// Container child hbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.list = new global::Gtk.TreeView ();
			this.list.CanFocus = true;
			this.list.Name = "list";
			this.GtkScrolledWindow.Add (this.list);
			this.hbox2.Add (this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.GtkScrolledWindow]));
			w3.Position = 0;
			// Container child hbox2.Gtk.Box+BoxChild
			this.vboxButtons = new global::Gtk.VBox ();
			this.vboxButtons.Name = "vboxButtons";
			this.vboxButtons.Spacing = 6;
			// Container child vboxButtons.Gtk.Box+BoxChild
			this.buttonApplyRemove = new global::Gtk.Button ();
			this.buttonApplyRemove.CanFocus = true;
			this.buttonApplyRemove.Name = "buttonApplyRemove";
			this.buttonApplyRemove.UseUnderline = true;
			this.buttonApplyRemove.Label = global::Mono.Unix.Catalog.GetString ("Apply and Remove");
			this.vboxButtons.Add (this.buttonApplyRemove);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vboxButtons [this.buttonApplyRemove]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Container child vboxButtons.Gtk.Box+BoxChild
			this.buttonApply = new global::Gtk.Button ();
			this.buttonApply.CanFocus = true;
			this.buttonApply.Name = "buttonApply";
			this.buttonApply.UseUnderline = true;
			this.buttonApply.Label = global::Mono.Unix.Catalog.GetString ("Apply");
			this.vboxButtons.Add (this.buttonApply);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vboxButtons [this.buttonApply]));
			w5.Position = 1;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vboxButtons.Gtk.Box+BoxChild
			this.buttonBranch = new global::Gtk.Button ();
			this.buttonBranch.CanFocus = true;
			this.buttonBranch.Name = "buttonBranch";
			this.buttonBranch.UseUnderline = true;
			this.buttonBranch.Label = global::Mono.Unix.Catalog.GetString ("Convert to Branch");
			this.vboxButtons.Add (this.buttonBranch);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vboxButtons [this.buttonBranch]));
			w6.Position = 2;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vboxButtons.Gtk.Box+BoxChild
			this.hseparator1 = new global::Gtk.HSeparator ();
			this.hseparator1.Name = "hseparator1";
			this.vboxButtons.Add (this.hseparator1);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vboxButtons [this.hseparator1]));
			w7.Position = 3;
			w7.Expand = false;
			w7.Fill = false;
			// Container child vboxButtons.Gtk.Box+BoxChild
			this.buttonDelete = new global::Gtk.Button ();
			this.buttonDelete.CanFocus = true;
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.UseStock = true;
			this.buttonDelete.UseUnderline = true;
			this.buttonDelete.Label = "gtk-remove";
			this.vboxButtons.Add (this.buttonDelete);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vboxButtons [this.buttonDelete]));
			w8.Position = 4;
			w8.Expand = false;
			w8.Fill = false;
			this.hbox2.Add (this.vboxButtons);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.vboxButtons]));
			w9.Position = 1;
			w9.Expand = false;
			w9.Fill = false;
			w1.Add (this.hbox2);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(w1 [this.hbox2]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Internal child MonoDevelop.VersionControl.Mercurial.ShelfManagerDialog.ActionArea
			global::Gtk.HButtonBox w11 = this.ActionArea;
			w11.Name = "dialog1_ActionArea";
			w11.Spacing = 10;
			w11.BorderWidth = ((uint)(5));
			w11.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-close";
			this.AddActionWidget (this.buttonOk, -7);
			global::Gtk.ButtonBox.ButtonBoxChild w12 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w11 [this.buttonOk]));
			w12.Expand = false;
			w12.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 527;
			this.DefaultHeight = 393;
			this.Show ();
			this.buttonApplyRemove.Clicked += new global::System.EventHandler (this.OnButtonApplyRemoveClicked);
			this.buttonApply.Clicked += new global::System.EventHandler (this.OnButtonApplyClicked);
			this.buttonDelete.Clicked += new global::System.EventHandler (this.OnButtonDeleteClicked);
		}
	}
}
