using System;
using System.Collections.Generic;
using System.Threading;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;

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
				{"Tailviewer.Events.BusinessLogic.QuickInfoAnalyserConfiguration", typeof(QuickInfoAnalyserConfiguration)}
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
