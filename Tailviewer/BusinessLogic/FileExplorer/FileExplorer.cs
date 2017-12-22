using System;
using System.Diagnostics;

namespace Tailviewer.BusinessLogic.FileExplorer
{
	internal sealed class FileExplorer : IFileExplorer
	{
		public void SelectFile(string path, IProgress<Percentage> progressReporter = null)
		{
			string argument = string.Format(@"/select, {0}", path);
			Process.Start("explorer.exe", argument);
		}

		public void OpenFolder(string path, IProgress<Percentage> progressReporter = null)
		{
			string argument = string.Format(@"/open, {0}", path);
			Process.Start("explorer.exe", argument);
		}
	}
}