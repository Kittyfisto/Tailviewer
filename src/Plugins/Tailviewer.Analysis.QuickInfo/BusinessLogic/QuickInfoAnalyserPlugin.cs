using System;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Analysis.QuickInfo.BusinessLogic
{
	public sealed class QuickInfoAnalyserPlugin
		: ILogAnalyserPlugin
	{
		public static readonly AnalyserPluginId Id = new AnalyserPluginId("Tailviewer.Analyser.QuickInfo");

		AnalyserPluginId ILogAnalyserPlugin.Id => Id;

		public ILogAnalyser Create(IServiceContainer services, ILogFile source, ILogAnalyserConfiguration configuration)
		{
			return new QuickInfoAnalyser(services,
				source,
				TimeSpan.FromMilliseconds(100),
				(QuickInfoAnalyserConfiguration)configuration);
		}
	}
}
