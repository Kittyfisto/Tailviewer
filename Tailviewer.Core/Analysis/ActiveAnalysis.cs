using System;
using System.Collections.Generic;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     Represents an <see cref="AnalysisTemplate" /> as well as the selection
	///     of data sources.
	/// </summary>
	public sealed class ActiveAnalysis
		: ISerializableType
	{
		private readonly List<DataSourceId> _dataSources;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		public ActiveAnalysis()
		{
			Template = new AnalysisTemplate();
			_dataSources = new List<DataSourceId>();
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <paramref name="template" />
		public ActiveAnalysis(AnalysisTemplate template)
		{
			if (template == null)
				throw new ArgumentNullException(nameof(template));

			Template = template;
			_dataSources = new List<DataSourceId>();
		}

		/// <summary>
		/// The template the analysis is based on.
		/// </summary>
		public AnalysisTemplate Template { get; }

		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Template", Template);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Template", Template);
		}
	}
}