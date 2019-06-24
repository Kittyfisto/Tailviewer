using System;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Analysis.Count.BusinessLogic
{
	public sealed class LogEntryCountAnalyserPlugin
		: ILogAnalyserPlugin
	{
		public static readonly AnalyserPluginId Id = new AnalyserPluginId("Tailviewer.Analyser.LogEntryCount");

		AnalyserPluginId ILogAnalyserPlugin.Id => Id;

		public ILogAnalyser Create(IServiceContainer services, ILogFile source, ILogAnalyserConfiguration configuration)
		{
			return new LogEntryCountAnalyser(services,
				source,
				TimeSpan.FromMilliseconds(100),
				(LogEntryCountAnalyserConfiguration)configuration);
		}
	}
}
