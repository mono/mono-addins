using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

class GLibrary
{

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetDllDirectory(string lpPathName);

	private static Dictionary<Library, IntPtr> _libraries;
	private static HashSet<Library> _librariesNotFound;
	private static Dictionary<string, IntPtr> _customlibraries;
	private static Dictionary<Library, string[]> _libraryDefinitions;

	static GLibrary()
	{
		_customlibraries = new Dictionary<string, IntPtr>();
		_librariesNotFound = new HashSet<Library>();
		_libraries = new Dictionary<Library, IntPtr>();
		_libraryDefinitions = new Dictionary<Library, string[]>();
		_libraryDefinitions[Library.Intl] = new[] {"libintl-8.dll", "libc.so.6", "libintl.8.dylib", "intl"};
	}

	public static IntPtr Load(Library library)
	{
		if (_libraries.TryGetValue(library, out var ret))
			return ret;

		if (TryGet(library, out ret)) return ret;

		var err = library + ": " + string.Join(", ", _libraryDefinitions[library]);

		throw new DllNotFoundException(err);

	}

	public static bool IsSupported(Library library) => TryGet(library, out var __);

	static bool TryGet(Library library, out IntPtr ret)
	{
		ret = IntPtr.Zero;

		if (_libraries.TryGetValue(library, out ret)) {
			return true;
		}

		if (_librariesNotFound.Contains(library)) {
			return false;
		}

		if (FuncLoader.IsWindows) {
			ret = FuncLoader.LoadLibrary(_libraryDefinitions[library][0]);

			if (ret == IntPtr.Zero) {
				SetDllDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
				ret = FuncLoader.LoadLibrary(_libraryDefinitions[library][0]);
			}
		} else if (FuncLoader.IsOSX) {
			ret = FuncLoader.LoadLibrary(_libraryDefinitions[library][2]);

			if (ret == IntPtr.Zero) {
				ret = FuncLoader.LoadLibrary("/usr/local/lib/" + _libraryDefinitions[library][2]);
				if (ret == IntPtr.Zero) {
					ret = FuncLoader.LoadLibrary("/opt/homebrew/lib/" + _libraryDefinitions[library][2]);
				}
			}
		} else
			ret = FuncLoader.LoadLibrary(_libraryDefinitions[library][1]);

		if (ret == IntPtr.Zero) {
			for (var i = 0; i < _libraryDefinitions[library].Length; i++) {
				ret = FuncLoader.LoadLibrary(_libraryDefinitions[library][i]);

				if (ret != IntPtr.Zero)
					break;
			}
		}

		if (ret != IntPtr.Zero) {
			_libraries[library] = ret;
		} else {
			_librariesNotFound.Add(library);
		}

		return ret != IntPtr.Zero;
	}

}
