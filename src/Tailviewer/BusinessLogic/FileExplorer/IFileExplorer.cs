namespace Tailviewer.BusinessLogic.FileExplorer
{
	public interface IFileExplorer
	{
		void SelectFiles(string folder, params string[] filesToSelect);

		void SelectFile(string filePath);

		void OpenFolder(string folderPath);
	}
}