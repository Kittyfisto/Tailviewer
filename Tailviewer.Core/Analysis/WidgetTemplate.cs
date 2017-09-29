using System;
using System.Diagnostics.Contracts;
using Tailviewer.Templates.Analysis;
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
		private WidgetId _id;
		private string _title;
		private AnalyserId _analyserId;
		private IWidgetConfiguration _viewConfiguration;

		/// <summary>
		///     The title of the widget.
		/// </summary>
		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		/// <summary>
		///     The id of this widget.
		/// </summary>
		public WidgetId Id
		{
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>
		///     The id of the analyser instance which is coupled
		///     with this widget.
		/// </summary>
		public AnalyserId AnalyserId
		{
			get { return _analyserId; }
			set { _analyserId = value; }
		}

		/// <summary>
		///     The configuration of the view (widget).
		/// </summary>
		public IWidgetConfiguration ViewConfiguration
		{
			get { return _viewConfiguration; }
			set { _viewConfiguration = value; }
		}

		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Id", _id);
			writer.WriteAttribute("Title", _title);
			writer.WriteAttribute("ViewConfiguration", _viewConfiguration);
			writer.WriteAttribute("AnalyserId", _analyserId);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Id", out _id);
			reader.TryReadAttribute("Title", out _title);
			reader.TryReadAttribute("ViewConfiguration", out _viewConfiguration);
			reader.TryReadAttribute("AnalyserId", out _analyserId);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		///     Creates a deep clone of this template.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public WidgetTemplate Clone()
		{
			return new WidgetTemplate
			{
				Id = _id,
				Title = _title,
				AnalyserId = AnalyserId,
				ViewConfiguration = _viewConfiguration?.Clone() as IWidgetConfiguration
			};
		}
	}
}