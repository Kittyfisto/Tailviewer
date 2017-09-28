using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     The template for a widget:
	///     - widget "view" configuration
	///     - analysis factory id (defines which analyser was used)
	///     - analyser id (defines the actual analyser instance)
	///     - analysis configuration
	/// </summary>
	public sealed class WidgetTemplate
		: IWidgetTemplate
	{
		private LogAnalyserFactoryId _analyserFactoryId;
		private LogAnalyserId _analyserId;
		private ILogAnalyserConfiguration _analysisConfiguration;

		private WidgetId _id;
		private string _title;
		private IWidgetConfiguration _viewConfiguration;

		/// <summary>
		///     Initializes this template.
		/// </summary>
		public WidgetTemplate()
		{
		}

		/// <summary>
		///     Initializes this template.
		/// </summary>
		public WidgetTemplate(WidgetId id, ILogAnalyserConfiguration logAnalyserConfiguration, IWidgetConfiguration viewConfiguration)
		{
			_id = id;
			_analysisConfiguration = logAnalyserConfiguration;
			_viewConfiguration = viewConfiguration;
		}

		/// <summary>
		///     The title of the widget.
		/// </summary>
		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		/// <summary>
		///     The configuration of the view (widget).
		/// </summary>
		public IWidgetConfiguration ViewConfiguration => _viewConfiguration;

		/// <summary>
		///     The configuration of the analyser.
		/// </summary>
		public ILogAnalyserConfiguration AnalysisConfiguration => _analysisConfiguration;

		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Id", _id);
			writer.WriteAttribute("Title", _title);
			writer.WriteAttribute("ViewConfiguration", _viewConfiguration);

			writer.WriteAttribute("AnalyserFactoryId", _analyserFactoryId);
			writer.WriteAttribute("AnalyserId", _analyserId);
			writer.WriteAttribute("AnalysisConfiguration", _analysisConfiguration);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Id", out _id);
			reader.TryReadAttribute("Title", out _title);
			reader.TryReadAttribute("ViewConfiguration", out _viewConfiguration);

			reader.TryReadAttribute("AnalyserFactoryId", out _analyserFactoryId);
			reader.TryReadAttribute("AnalyserId", out _analyserId);
			reader.TryReadAttribute("AnalysisConfiguration", out _analysisConfiguration);
		}
	}
}