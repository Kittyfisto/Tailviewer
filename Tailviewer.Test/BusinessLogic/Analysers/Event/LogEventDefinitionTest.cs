using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Analysers.Event;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic.Analysers.Event
{
	[TestFixture]
	public sealed class LogEventDefinitionTest
	{
		[Test]
		public void TestTryExtractEventFrom1()
		{
			var definition = new LogEventDefinition(@"Found channel ([0-9]*\.?[0-9]MHz)");
			definition.TryExtractEventFrom(new LogLine(0, 0, "Found channel 816MHz", LevelFlags.Info));
		}
	}
}