
using System;
using Gtk;

namespace Mono.Addins.Gui
{
	partial class NewSiteDialog : Dialog
	{
		public NewSiteDialog ()
		{
			Build ();
			CheckValues ();
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			Destroy ();
		}
		
		public string Url {
			get {
				if (btnOnlineRep.Active)
					return urlText.Text;
				else if (pathEntry.Text != "")
					return "file://" + pathEntry.Text;
				else
					return "";
			}
		}
		
		void CheckValues ()
		{
			btnOk.Sensitive = (Url != "");
		}
		
		public new bool Run ()
		{
			ShowAll ();
			return ((ResponseType) base.Run ()) == ResponseType.Ok;
		}
		
		protected void OnTextChanged (object sender, EventArgs args)
		{
			CheckValues ();
		}
		
		protected void OnClose (object sender, EventArgs args)
		{
			Destroy ();
		}
		
		protected void OnOptionClicked (object sender, EventArgs e)
		{
			if (btnOnlineRep.Active) {
				urlText.Sensitive = true;
				pathEntry.Sensitive = false;
			} else {
				urlText.Sensitive = false;
				pathEntry.Sensitive = true;
			}
			CheckValues ();
		}
	}
}
