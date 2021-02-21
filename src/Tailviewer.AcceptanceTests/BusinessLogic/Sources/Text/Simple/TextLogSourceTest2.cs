using System.Text;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.Sources.Text.Simple;
using Tailviewer.Test.BusinessLogic.Sources;

namespace Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text.Simple
{
	[TestFixture]
	public sealed class TextLogSourceTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogSource CreateEmpty(ITaskScheduler taskScheduler)
		{
			return new TextLogSource(taskScheduler, "", LogFileFormats.GenericText, Encoding.Default);
		}

		#endregion
	}
}