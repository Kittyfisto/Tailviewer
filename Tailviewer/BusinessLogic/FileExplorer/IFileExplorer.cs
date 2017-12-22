using System;

namespace Tailviewer.BusinessLogic.FileExplorer
{
	public interface IFileExplorer
	{
		void SelectFile(string path, IProgress<Percentage> progressReporter = null);

		void OpenFolder(string path, IProgress<Percentage> progressReporter = null);
	}
}