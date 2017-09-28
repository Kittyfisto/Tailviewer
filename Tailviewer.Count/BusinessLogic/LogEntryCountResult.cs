using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Count.BusinessLogic
{
	public sealed class LogEntryCountResult
		: ILogAnalysisResult
	{
		private long _count;

		public long Count
		{
			get { return _count; }
			set { _count = value; }
		}

		public object Clone()
		{
			return new LogEntryCountResult {Count = Count};
		}

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Count", Count);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Count", out _count);
		}
	}
}