namespace Tailviewer.PluginRepository
{
	public enum ExitCode
	{
		Success = 0,

		GenericFailure = -1,

		InvalidUserName = -10,
		InvalidUserToken = -12,

		FileNotFound = -20,
		DirectoryNotFound = -21,

		InvalidAddress = -30,

		UnhandledException = int.MinValue
	}
}
