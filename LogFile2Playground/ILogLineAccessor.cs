using Tailviewer.BusinessLogic.LogFiles;

namespace LogFile2Playground
{
	public interface ILogLineAccessor
	{
		LogLineResponse Request(LogFileSection section);
	}
}