//
// Services.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using Gtk;
using Mono.Unix;
using Mono.Addins.Setup;
using Mono.Addins.Description;
using System.Linq;
using System.Collections.Generic;

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
			MessageDialog md = new MessageDialog (null, DialogFlags.Modal | DialogFlags.DestroyWithParent, MessageType.Question, ButtonsType.YesNo, question);
			try {
				int response = md.Run ();
				return ((ResponseType) response == ResponseType.Yes);
			} finally {
				md.Destroy ();
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

			if (parent != null) {
				CenterWindow (dlg, parent);
			}

			if (modal) {
				dlg.Run ();
				dlg.Destroy ();
			} else
				dlg.Show ();
		}
		
		public struct MissingDepInfo
		{
			public string Addin;
			public string Required;
			public string Found;
		}
		
		public static IEnumerable<MissingDepInfo> GetMissingDependencies (Addin addin)
		{
			IEnumerable<Addin> allAddins = AddinManager.Registry.GetAddins ().Union (AddinManager.Registry.GetAddinRoots ());
			foreach (var dep in addin.Description.MainModule.Dependencies) {
				AddinDependency adep = dep as AddinDependency;
				if (adep != null) {
					if (!allAddins.Any (a => Addin.GetIdName (a.Id) == Addin.GetIdName (adep.FullAddinId) &&  a.SupportsVersion (adep.Version))) {
						Addin found = allAddins.FirstOrDefault (a => Addin.GetIdName (a.Id) == Addin.GetIdName (adep.FullAddinId));
						yield return new MissingDepInfo () { Addin = Addin.GetIdName (adep.FullAddinId), Required = adep.Version, Found = found != null ? found.Version : null };
					}
				}
			}
		}
		
		public static Gdk.Pixbuf AddIconOverlay (Gdk.Pixbuf target, Gdk.Pixbuf overlay)
		{
			Gdk.Pixbuf res = new Gdk.Pixbuf (target.Colorspace, target.HasAlpha, target.BitsPerSample, target.Width, target.Height);
			res.Fill (0);
			target.CopyArea (0, 0, target.Width, target.Height, res, 0, 0);
			overlay.Composite (res, 0, 0, overlay.Width, overlay.Height, 0, 0, 1, 1, Gdk.InterpType.Bilinear, 255);
			return res;
		}
		
		public static Gdk.Pixbuf DesaturateIcon (Gdk.Pixbuf source)
		{
			Gdk.Pixbuf dest = new Gdk.Pixbuf (source.Colorspace, source.HasAlpha, source.BitsPerSample, source.Width, source.Height);
			dest.Fill (0);
			source.SaturateAndPixelate (dest, 0, false);
			return dest;
		}
		
		public static Gdk.Pixbuf FadeIcon (Gdk.Pixbuf source)
		{
			Gdk.Pixbuf result = source.Copy ();
			result.Fill (0);
			result = result.AddAlpha (true, 0, 0, 0);
			source.Composite (result, 0, 0, source.Width, source.Height, 0, 0, 1, 1, Gdk.InterpType.Bilinear, 128);
			return result;
		}
		
		/// <summary>
		/// Positions a dialog relative to its parent on platforms where default placement is known to be poor.
		/// </summary>
		public static void PlaceDialog (Window child, Window parent)
		{
			CenterWindow (child, parent);
		}
		
		/// <summary>Centers a window relative to its parent.</summary>
		static void CenterWindow (Window child, Window parent)
		{
			if (child == null || parent == null)
				return;

			child.Child.Show ();
			int w, h, winw, winh, x, y, winx, winy;
			child.GetSize (out w, out h);
			parent.GetSize (out winw, out winh);
			parent.GetPosition (out winx, out winy);
			x = System.Math.Max (0, (winw - w) /2) + winx;
			y = System.Math.Max (0, (winh - h) /2) + winy;
			child.Move (x, y);
		}
	}
}
