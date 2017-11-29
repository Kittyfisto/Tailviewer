using Tailviewer.BusinessLogic.LogFiles;

namespace LogFile2Playground
{
	public struct LogLineResponse
	{
		public LogLine[] Lines;
		public LogFileSection ActualSection;
	}
}