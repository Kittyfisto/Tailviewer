using System.Collections.Generic;

namespace Tailviewer.Settings.Analysis
{
	/// <summary>
	///     A snapshot of an analysis that consists of the analyses' template
	///     as well as the results of each and every analyser.
	/// </summary>
	public sealed class AnalysisSnapshot
		: ISerializableType
	{
		private readonly List<AnalyserResult> _results;

		public AnalysisSnapshot()
		{
			Template = new AnalysisTemplate();
			_results = new List<AnalyserResult>();
		}

		public AnalysisTemplate Template { get; }

		public IEnumerable<AnalyserResult> Results => _results;

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Template", Template);
			writer.WriteAttribute("Results", _results);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Template", Template);
			reader.TryReadAttribute("Results", _results);
		}
	}
}