namespace Tailviewer.BusinessLogic.Filesystem
{
	public interface IDirectoryInfo
	{
		string Name { get; }
		bool Exists { get; }
	}
}