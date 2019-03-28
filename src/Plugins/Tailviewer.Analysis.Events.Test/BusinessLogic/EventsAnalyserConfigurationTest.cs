using NUnit.Framework;
using Tailviewer.Analysis.Events.BusinessLogic;
using Tailviewer.Core;

namespace Tailviewer.Analysis.Events.Test.BusinessLogic
{
	[TestFixture]
	public sealed class EventsAnalyserConfigurationTest
	{
		[Test]
		public void TestRoundtrip()
		{
			var config = new EventsLogAnalyserConfiguration
			{
				MaxEvents = 9999,
				Events =
				{
					new EventConfiguration
					{
						Name = "My custom event",
						FilterExpression = "%d",
					}
				}
			};
			var actualConfig = Roundtrip(config);
			//actualConfig.MaxEvents.Should().Be(9999);
			//actualConfig.Events.Count.Should().Be(1);
			//actualConfig.Events[0].Name.Should().Be("My custom event");
			//actualConfig.Events[0].FilterExpression.Should().Be("%d");
		}

		private EventsLogAnalyserConfiguration Roundtrip(EventsLogAnalyserConfiguration config)
		{
			return config.Roundtrip(typeof(EventConfiguration));
		}
	}
}