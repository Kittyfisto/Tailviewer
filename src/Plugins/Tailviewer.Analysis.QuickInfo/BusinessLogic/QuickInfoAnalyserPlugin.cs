using System;
using System.Collections.Generic;
using System.Threading;
using Tailviewer.Analysis.QuickInfo.Ui;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Analysis.QuickInfo.BusinessLogic
{
	public sealed class QuickInfoAnalyserPlugin
		: ILogAnalyserPlugin
	{
		public static readonly AnalyserPluginId Id = new AnalyserPluginId("Tailviewer.Analyser.QuickInfo");

		AnalyserPluginId ILogAnalyserPlugin.Id => Id;

		public IEnumerable<KeyValuePair<string, Type>> SerializableTypes => 
			new Dictionary<string, Type>
			{
				{"Tailviewer.QuickInfo.BusinessLogic.QuickInfoAnalyserConfiguration", typeof(QuickInfoAnalyserConfiguration)},
				{"Tailviewer.QuickInfo.Ui.QuickInfoWidgetConfiguration", typeof(QuickInfoWidgetConfiguration)}
			};

		public ILogAnalyser Create(ITaskScheduler scheduler, ILogFile source, ILogAnalyserConfiguration configuration)
		{
			return new QuickInfoAnalyser(scheduler,
				source,
				TimeSpan.FromMilliseconds(100),
				(QuickInfoAnalyserConfiguration)configuration);
		}
	}
}
