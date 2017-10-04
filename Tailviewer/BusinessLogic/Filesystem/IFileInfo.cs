namespace Tailviewer.BusinessLogic.Filesystem
{
	public interface IFileInfo
	{
		string Name { get; }
		long Length { get; }
		bool IsReadOnly { get; }
		bool Exists { get; }
	}
}