namespace Tailviewer.Archiver
{
	public enum ExitCode
	{
		Success = 0,

		GenericFailure = -1,

		RemotePublishDisabled = -10,
		CorruptPlugin = -11,
		InvalidUserToken = -12,
		PluginAlreadyPublished = -13,
		ConnectionError = -14,

		FileNotFound = -20,
		DirectoryNotFound = -21,

		UnhandledException = int.MinValue
	}
}
