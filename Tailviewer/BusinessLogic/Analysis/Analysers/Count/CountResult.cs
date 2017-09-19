namespace Tailviewer.BusinessLogic.Analysis.Analysers.Count
{
	public sealed class CountResult
		: ILogAnalysisResult
	{
		public long Count { get; set; }

		public object Clone()
		{
			return new CountResult {Count = Count};
		}
	}
}