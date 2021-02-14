using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.LogFiles.Text;
using Tailviewer.Test;
using Tailviewer.Test.BusinessLogic.LogFiles;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles.Text
{
	[TestFixture]
	public sealed class TextLogFileTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogFile CreateEmpty(ITaskScheduler taskScheduler)
		{
			var serviceContainer = new ServiceContainer();
			serviceContainer.RegisterInstance(taskScheduler);
			serviceContainer.RegisterInstance<ILogFileFormatMatcher>(new SimpleLogFileFormatMatcher(LogFileFormats.GenericText));
			serviceContainer.RegisterInstance<ITextLogFileParserPlugin>(new SimpleTextLogFileParserPlugin());
			return new TextLogFile(serviceContainer, "");
		}

		#endregion
	}
}