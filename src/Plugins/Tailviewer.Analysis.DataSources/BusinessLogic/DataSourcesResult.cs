using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Analysis.DataSources.BusinessLogic
{
	/// <summary>
	///     The analysis result of a <see cref="DataSourcesAnalyser" />: Describes the data sources which were used during the analysis.
	/// </summary>
	[DataContract]
	public sealed class DataSourcesResult
		: ILogAnalysisResult
	{
		public DataSourcesResult()
		{
			DataSources = new List<DataSource>();
		}

		public List<DataSource> DataSources { get; }

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("DataSources", DataSources);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("DataSources", DataSources);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public DataSourcesResult Clone()
		{
			var clone = new DataSourcesResult();
			clone.DataSources.AddRange(DataSources.Select(x => x.Clone()));
			return clone;
		}
	}
}