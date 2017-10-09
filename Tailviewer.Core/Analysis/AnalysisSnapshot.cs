using System.Collections.Generic;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     A snapshot of an analysis that consists of the analyses' template
	///     as well as the results of each and every analyser.
	/// </summary>
	public sealed class AnalysisSnapshot
		: ISerializableType
	{
		private readonly List<AnalyserResult> _results;

		/// <summary>
		///     Initializes this snapshot.
		/// </summary>
		public AnalysisSnapshot()
		{
			Template = new AnalysisTemplate();
			_results = new List<AnalyserResult>();
		}

		/// <summary>
		///     Initializes this snapshot.
		/// </summary>
		public AnalysisSnapshot(AnalysisTemplate template, IEnumerable<AnalyserResult> results)
		{
			Template = template;
			_results = new List<AnalyserResult>(results);
		}

		/// <summary>
		///     The template used to create the analysis, from which this snapshot was created.
		/// </summary>
		public AnalysisTemplate Template { get; }

		/// <summary>
		///     The results of all the analysers from when this snapshot
		///     was created.
		/// </summary>
		public IEnumerable<AnalyserResult> Results => _results;

		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Template", Template);
			writer.WriteAttribute("Results", _results);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Template", Template);
			reader.TryReadAttribute("Results", _results);
		}
	}
}