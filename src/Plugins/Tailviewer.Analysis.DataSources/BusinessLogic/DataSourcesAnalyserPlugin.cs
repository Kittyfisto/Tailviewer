using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Analysis.DataSources.BusinessLogic
{
	public sealed class DataSourcesAnalyserPlugin
		: IDataSourceAnalyserPlugin
	{
		public static readonly AnalyserPluginId Id = new AnalyserPluginId("Tailviewer.Analyser.DataSources");

		AnalyserPluginId IDataSourceAnalyserPlugin.Id => Id;

		public IDataSourceAnalyser Create(AnalyserId id, ILogFile logFile, ILogAnalyserConfiguration configuration)
		{
			return new DataSourcesAnalyser(id);
		}

		public IEnumerable<KeyValuePair<string, Type>> SerializableTypes => new Dictionary<string, Type>
		{
			{ "Tailviewer.DataSources.BusinessLogic.DataSourcesResult", typeof(DataSourcesResult) },
			{ "Tailviewer.DataSources.BusinessLogic.DataSource", typeof(DataSource) }
		};
	}
}
