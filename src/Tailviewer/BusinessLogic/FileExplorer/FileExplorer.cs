using System.Diagnostics;

namespace Tailviewer.BusinessLogic.FileExplorer
{
	internal sealed class FileExplorer : IFileExplorer
	{
		public void SelectFile( string filePath)
		{
			Process.Start("explorer.exe", $@"/select, {filePath}");
		}
		public void SelectFiles(string folder, params string[] filesToSelect)
		{
			NativeMethods.OpenFolderAndSelectFiles(folder, filesToSelect);
		}
		public void OpenFolder(string folderPath)
		{
			Process.Start("explorer.exe", $@"/open, {folderPath}");
		}
	}
}