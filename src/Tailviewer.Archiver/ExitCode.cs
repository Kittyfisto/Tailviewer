namespace Tailviewer.Archiver
{
	public enum ExitCode
	{
		Success = 0,
		FileNotFound = -1,
		DirectoryNotFound = -2,

		RemotePublishDisabled = -10,
		CorruptPlugin = -11,
		InvalidUserToken = -12,
		PluginAlreadyPublished = -13,

		UnhandledException = int.MinValue
	}
}
