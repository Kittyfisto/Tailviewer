using System;

namespace Tailviewer.BusinessLogic.FileExplorer
{
	public interface IFileExplorer
	{
		void SelectFile(string path);

		void OpenFolder(string path);
	}
}