using System;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo
{
	public sealed class QuickInfoAnalyserFactory
		: ILogAnalyserFactory
	{
		public static readonly LogAnalyserFactoryId Id = new LogAnalyserFactoryId("Tailviewer.Analyser.QuickInfo");

		LogAnalyserFactoryId ILogAnalyserFactory.Id => Id;

		public ILogAnalyser Create(ITaskScheduler scheduler, ILogFile source, ILogAnalyserConfiguration configuration)
		{
			return new QuickInfoAnalyser(scheduler,
				source,
				TimeSpan.FromMilliseconds(100),
				(QuickInfoAnalyserConfiguration)configuration);
		}
	}
}
