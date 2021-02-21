using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Test;

namespace Tailviewer.AcceptanceTests.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class FileDataSourceAcceptanceTest
	{
		private ManualTaskScheduler _scheduler;
		private PluginLogFileFactory _logFileFactory;

		[SetUp]
		public void SetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new SimplePluginLogFileFactory(_scheduler);
		}
	}
}
