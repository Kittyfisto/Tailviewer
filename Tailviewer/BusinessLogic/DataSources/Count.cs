namespace Tailviewer.BusinessLogic.DataSources
{
	public interface ICount
	{
		int LogLineCount { get; }
		int LogEntryCount { get; }
	}
}