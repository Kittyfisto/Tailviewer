using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Plugins;
using Tailviewer.Test;
using Tailviewer.Test.BusinessLogic.LogFiles;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles.Text
{
	[TestFixture]
	public sealed class TextLogFileTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogSource CreateEmpty(ITaskScheduler taskScheduler)
		{
			var serviceContainer = new ServiceContainer();
			serviceContainer.RegisterInstance(taskScheduler);
			serviceContainer.RegisterInstance<ILogFileFormatMatcher>(new SimpleLogFileFormatMatcher(LogFileFormats.GenericText));
			serviceContainer.RegisterInstance<ILogEntryParserPlugin>(new SimpleLogEntryParserPlugin());
			return new TextLogSource(serviceContainer, "");
		}

		#endregion
	}
}