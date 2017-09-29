using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     The result of a log analyser - only used to persist and restore it via
	///     <see cref="IReader" /> and <see cref="IWriter" />.
	/// </summary>
	public sealed class AnalyserResult
		: ISerializableType
	{
		private AnalyserId _analyserId;
		private ILogAnalysisResult _result;

		/// <summary>
		///     The id of the analyser that produced the result.
		/// </summary>
		public AnalyserId AnalyserId => _analyserId;

		/// <summary>
		///     The result of the analyser.
		/// </summary>
		public ILogAnalysisResult Result => _result;

		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("AnalyserId", _analyserId);
			writer.WriteAttribute("Result", _result);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("AnalyserId", out _analyserId);
			reader.TryReadAttribute("Result", out _result);
		}
	}
}