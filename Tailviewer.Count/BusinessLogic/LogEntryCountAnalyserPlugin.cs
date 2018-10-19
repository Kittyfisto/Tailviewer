using System;
using System.Collections.Generic;
using System.Threading;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Count.BusinessLogic
{
	public sealed class LogEntryCountAnalyserPlugin
		: ILogAnalyserPlugin
	{
		public static readonly LogAnalyserFactoryId Id = new LogAnalyserFactoryId("Tailviewer.Analyser.LogEntryCount");

		LogAnalyserFactoryId ILogAnalyserPlugin.Id => Id;

		public IEnumerable<KeyValuePair<string, Type>> SerializableTypes => 
			new Dictionary<string, Type>
			{
				{"Tailviewer.Events.BusinessLogic.LogEntryCountAnalyserConfiguration", typeof(LogEntryCountAnalyserConfiguration)}
			};

		public ILogAnalyser Create(ITaskScheduler scheduler, ILogFile source, ILogAnalyserConfiguration configuration)
		{
			return new LogEntryCountAnalyser(scheduler,
				source,
				TimeSpan.FromMilliseconds(100),
				(LogEntryCountAnalyserConfiguration)configuration);
		}
	}
}
