using System;
using System.Collections.Generic;

namespace Tailviewer.Settings.Analysis
{
	/// <summary>
	/// Represents an <see cref="AnalysisTemplate"/> as well as the selection
	/// of data sources.
	/// </summary>
	public sealed class ActiveAnalysis
		: ISerializableType
	{
		private readonly AnalysisTemplate _template;
		private readonly List<DataSourceId> _dataSources;

		public ActiveAnalysis()
		{
			_template = new AnalysisTemplate();
			_dataSources = new List<DataSourceId>();
		}

		public ActiveAnalysis(AnalysisTemplate template)
		{
			if (template == null)
				throw new ArgumentNullException(nameof(template));

			_template = template;
			_dataSources = new List<DataSourceId>();
		}

		public AnalysisTemplate Template => _template;

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Template", _template);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Template", _template);
		}
	}
}