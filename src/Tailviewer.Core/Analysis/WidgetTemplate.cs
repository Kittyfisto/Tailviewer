using System;
using System.Diagnostics.Contracts;
using Tailviewer.BusinessLogic.Analysis;
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
		private IWidgetConfiguration _configuration;
		private AnalyserPluginId _analyserPluginId;

		/// <inheritdoc />
		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		/// <inheritdoc />
		public WidgetId Id
		{
			get { return _id; }
			set { _id = value; }
		}
		
		/// <inheritdoc />
		public AnalyserId AnalyserId
		{
			get { return _analyserId; }
			set { _analyserId = value; }
		}

		/// <inheritdoc />
		public AnalyserPluginId AnalyserPluginId
		{
			get { return _analyserPluginId; }
			set { _analyserPluginId = value; }
		}

		/// <inheritdoc />
		public IWidgetConfiguration Configuration
		{
			get { return _configuration; }
			set { _configuration = value; }
		}

		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Id", _id);
			writer.WriteAttribute("Title", _title);
			writer.WriteAttribute("Configuration", _configuration);
			writer.WriteAttribute("AnalyserId", _analyserId);
			writer.WriteAttribute("FactoryId", _analyserPluginId);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Id", out _id);
			reader.TryReadAttribute("Title", out _title);
			reader.TryReadAttribute("Configuration", out _configuration);
			reader.TryReadAttribute("AnalyserId", out _analyserId);
			reader.TryReadAttribute("FactoryId", out _analyserPluginId);
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
				AnalyserPluginId = AnalyserPluginId,
				Configuration = _configuration?.Clone() as IWidgetConfiguration
			};
		}
	}
}