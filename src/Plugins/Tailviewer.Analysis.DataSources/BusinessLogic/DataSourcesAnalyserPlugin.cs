using System;
using System.Threading;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Analysis.DataSources.BusinessLogic
{
	public sealed class DataSourcesAnalyserPlugin
		: IDataSourceAnalyserPlugin
	{
		public static readonly AnalyserPluginId Id = new AnalyserPluginId("Tailviewer.Analyser.DataSources");

		AnalyserPluginId IDataSourceAnalyserPlugin.Id => Id;

		public IDataSourceAnalyser Create(IServiceContainer services, AnalyserId id, ILogFile logFile,
			ILogAnalyserConfiguration configuration)
		{
			return new DataSourcesAnalyser(id, TimeSpan.FromMilliseconds(100));
		}
	}
}
