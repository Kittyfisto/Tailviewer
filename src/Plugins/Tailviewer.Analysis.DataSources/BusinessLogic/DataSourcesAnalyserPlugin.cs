using System;
using System.Collections.Generic;
using System.Threading;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.DataSources.BusinessLogic
{
	public sealed class DataSourcesAnalyserPlugin
		: ILogAnalyserPlugin
	{
		public static readonly LogAnalyserFactoryId Id = new LogAnalyserFactoryId("Tailviewer.Analyser.DataSources");

		LogAnalyserFactoryId ILogAnalyserPlugin.Id => Id;

		public IEnumerable<KeyValuePair<string, Type>> SerializableTypes => new Dictionary<string, Type>
		{
			{ "Tailviewer.DataSources.BusinessLogic.DataSourcesResult", typeof(DataSourcesResult) },
			{ "Tailviewer.DataSources.BusinessLogic.DataSource", typeof(DataSource) }
		};

		public ILogAnalyser Create(ITaskScheduler scheduler, ILogFile source, ILogAnalyserConfiguration configuration)
		{
			return new DataSourcesAnalyser(source, TimeSpan.FromSeconds(1));
		}
	}
}
