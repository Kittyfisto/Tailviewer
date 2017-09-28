using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Settings.Analysis
{
	/// <summary>
	///     The result of a log analyser - only used to persist and restore it via
	///     <see cref="IReader" /> and <see cref="IWriter" />.
	/// </summary>
	public sealed class AnalyserResult
		: ISerializableType
	{
		private LogAnalyserId _analyserId;
		private ILogAnalysisResult _result;

		public LogAnalyserId AnalyserId => _analyserId;

		public ILogAnalysisResult Result => _result;

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("AnalyserId", _analyserId);
			writer.WriteAttribute("Result", _result);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("AnalyserId", out _analyserId);
			reader.TryReadAttribute("Result", out _result);
		}
	}
}