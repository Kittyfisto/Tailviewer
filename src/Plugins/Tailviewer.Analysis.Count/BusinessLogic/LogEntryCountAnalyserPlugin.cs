using System;
using System.Collections.Generic;
using System.Threading;
using Tailviewer.Analysis.Count.Ui;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Analysis.Count.BusinessLogic
{
	public sealed class LogEntryCountAnalyserPlugin
		: ILogAnalyserPlugin
	{
		public static readonly AnalyserPluginId Id = new AnalyserPluginId("Tailviewer.Analyser.LogEntryCount");

		AnalyserPluginId ILogAnalyserPlugin.Id => Id;

		public IEnumerable<KeyValuePair<string, Type>> SerializableTypes => 
			new Dictionary<string, Type>
			{
				{"Tailviewer.Count.BusinessLogic.LogEntryCountAnalyserConfiguration", typeof(LogEntryCountAnalyserConfiguration)},
				{"Tailviewer.Count.Ui.LogEntryCountWidgetConfiguration", typeof(LogEntryCountWidgetConfiguration) }
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
