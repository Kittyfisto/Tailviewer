using System;
using System.Collections.Generic;
using System.Threading;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Events.BusinessLogic
{
	public sealed class EventsLogAnalyserPlugin
		: ILogAnalyserPlugin
	{
		public static readonly LogAnalyserFactoryId Id = new LogAnalyserFactoryId("Tailviewer.Analyser.EventsLogAnalyser");

		LogAnalyserFactoryId ILogAnalyserPlugin.Id => Id;

		public IEnumerable<KeyValuePair<string, Type>> SerializableTypes => 
			new Dictionary<string, Type>
			{
				{"Tailviewer.Events.BusinessLogic.EventsLogAnalyserConfiguration", typeof(EventsLogAnalyserConfiguration)}
			};

		public ILogAnalyser Create(ITaskScheduler scheduler, ILogFile source, ILogAnalyserConfiguration configuration)
		{
			return new EventsLogAnalyser(scheduler,
				source,
				TimeSpan.FromMilliseconds(500),
				(EventsLogAnalyserConfiguration) configuration);
		}
	}
}
