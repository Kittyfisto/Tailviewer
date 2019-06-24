using System;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Analysis.Events.BusinessLogic
{
	public sealed class EventsLogAnalyserPlugin
		: ILogAnalyserPlugin
	{
		public static readonly AnalyserPluginId Id = new AnalyserPluginId("Tailviewer.Analyser.EventsLogAnalyser");

		AnalyserPluginId ILogAnalyserPlugin.Id => Id;

		public ILogAnalyser Create(IServiceContainer services, ILogFile source, ILogAnalyserConfiguration configuration)
		{
			return new EventsLogAnalyser(services,
				source,
				TimeSpan.FromMilliseconds(500),
				(EventsLogAnalyserConfiguration) configuration);
		}
	}
}
