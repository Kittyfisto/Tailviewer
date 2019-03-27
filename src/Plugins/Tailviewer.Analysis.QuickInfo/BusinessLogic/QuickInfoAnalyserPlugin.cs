using System;
using System.Collections.Generic;
using System.Threading;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.QuickInfo.Ui;

namespace Tailviewer.QuickInfo.BusinessLogic
{
	public sealed class QuickInfoAnalyserPlugin
		: ILogAnalyserPlugin
	{
		public static readonly LogAnalyserFactoryId Id = new LogAnalyserFactoryId("Tailviewer.Analyser.QuickInfo");

		LogAnalyserFactoryId ILogAnalyserPlugin.Id => Id;

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
