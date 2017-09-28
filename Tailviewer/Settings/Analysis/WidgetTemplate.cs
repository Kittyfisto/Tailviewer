using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Settings.Analysis
{
	/// <summary>
	/// The template for a widget:
	/// - widget "view" configuration
	/// - analysis factory id (defines which analyser was used)
	/// - analyser id (defines the actual analyser instance)
	/// - analysis configuration
	/// </summary>
	public sealed class WidgetTemplate
		: ISerializableType
	{
		private WidgetId _id;
		private LogAnalyserFactoryId _analyserFactoryId;
		private LogAnalyserId _analyserId;
		private string _title;
		private IWidgetConfiguration _widgetConfiguration;
		private ILogAnalyserConfiguration _analysisConfiguration;

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Id", _id);
			writer.WriteAttribute("Title", _title);
			writer.WriteAttribute("WidgetConfiguration", _widgetConfiguration);

			writer.WriteAttribute("AnalyserFactoryId", _analyserFactoryId);
			writer.WriteAttribute("AnalyserId", _analyserId);
			writer.WriteAttribute("AnalysisConfiguration", _analysisConfiguration);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Id", out _id);
			reader.TryReadAttribute("Title", out _title);
			reader.TryReadAttribute("WidgetConfiguration", out _widgetConfiguration);

			reader.TryReadAttribute("AnalyserFactoryId", out _analyserFactoryId);
			reader.TryReadAttribute("AnalyserId", out _analyserId);
			reader.TryReadAttribute("AnalysisConfiguration", out _analysisConfiguration);
		}
	}
}