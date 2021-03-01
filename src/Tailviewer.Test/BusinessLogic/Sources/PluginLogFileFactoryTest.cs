using System.Threading;
using NUnit.Framework;

namespace Tailviewer.Test.BusinessLogic.Sources
{
	[TestFixture]
	public sealed class PluginLogFileFactoryTest
	{
		private ManualTaskScheduler _scheduler;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_scheduler = new ManualTaskScheduler();
		}

		[Test]
		public void TestOpen1()
		{
			// How should tailviewer as a whole behave when a plugin says it's responsible for extension
			// x but then it crashes during Open? Should be just hide this crash from the user and
			// display it using TextLogFile instead? Or do we display an error to the user?
			// Depending on which way we decide, the current implementation (and this test)
			// need to be changed...
		}
	}
}