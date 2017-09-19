using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis.Analysers.Count
{
	public sealed class LineCountLogAnalyserFactory
		: ILogAnalyserFactory
	{
		public static readonly LogAnalyserFactoryId Id = new LogAnalyserFactoryId("Tailviewer.Analyser.LineCountLogAnalyser");

		LogAnalyserFactoryId ILogAnalyserFactory.Id => Id;

		public ILogAnalyser Create(ITaskScheduler scheduler, ILogFile source, ILogAnalyserConfiguration configuration)
		{
			return new LineCountLogAnalyser(scheduler, source, (LineCountAnalyserConfiguration)configuration);
		}
	}
}
