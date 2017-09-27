using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Count.BusinessLogic
{
	public sealed class LogEntryCountResult
		: ILogAnalysisResult
	{
		public long Count { get; set; }

		public object Clone()
		{
			return new LogEntryCountResult {Count = Count};
		}
	}
}