using System.IO;
using Mono.Addins.Database;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
	public class TestFileDatabase : TestBase
	{
		// https://github.com/mono/mono-addins/issues/105
		[Test]
		public void TestFilePathCaseOnWindows()
		{
			if (!Mono.Addins.Database.Util.IsWindows) {
				return;
			}

			var rootPath = Path.Combine(TempDir, nameof(TestFilePathCaseOnWindows));
			Directory.CreateDirectory(rootPath);

			var fileDatabase = new FileDatabase(rootPath);
			try {
				var folder = Path.Combine(rootPath, "Folder");
				Directory.CreateDirectory(folder);

				folder = char.ToUpperInvariant(folder[0]) + folder.Substring(1);
				var addinScanFolderInfo = new AddinScanFolderInfo(folder);
				addinScanFolderInfo.Write(fileDatabase, folder);

				folder = char.ToLowerInvariant(folder[0]) + folder.Substring(1);
				var actual = AddinScanFolderInfo.Read(fileDatabase, folder, folder);

				Assert.NotNull(actual);
				Assert.AreEqual(1, Directory.GetFiles(folder).Length);
			}
			finally {
				Directory.Delete(rootPath, recursive: true);
			}
		}
	}
}
