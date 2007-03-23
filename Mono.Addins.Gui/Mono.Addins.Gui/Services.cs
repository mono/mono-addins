
using System;
using Gtk;
using Mono.Unix;
using Mono.Addins.Setup;

namespace Mono.Addins.Gui
{
	internal class Services
	{
		public static bool InApplicationNamespace (SetupService service, string id)
		{
			return service.ApplicationNamespace == null || id.StartsWith (service.ApplicationNamespace + ".");
		}
		
		public static bool AskQuestion (string question)
		{
			using (MessageDialog md = new MessageDialog (null, DialogFlags.Modal | DialogFlags.DestroyWithParent, MessageType.Question, ButtonsType.YesNo, question)) {
				int response = md.Run ();
				md.Hide ();
				return ((ResponseType) response == ResponseType.Yes);
			}
		}
		
		public static void ShowError (Exception ex, string message, Window parent, bool modal)
		{
			ErrorDialog dlg = new ErrorDialog (parent);
			
			if (message == null) {
				if (ex != null)
					dlg.Message = string.Format (Catalog.GetString ("Exception occurred: {0}"), ex.Message);
				else {
					dlg.Message = "An unknown error occurred";
					dlg.AddDetails (Environment.StackTrace, false);
				}
			} else
				dlg.Message = message;
			
			if (ex != null) {
				dlg.AddDetails (string.Format (Catalog.GetString ("Exception occurred: {0}"), ex.Message) + "\n\n", true);
				dlg.AddDetails (ex.ToString (), false);
			}

			if (modal) {
				dlg.Run ();
				dlg.Dispose ();
			} else
				dlg.Show ();
		}
	}
}
