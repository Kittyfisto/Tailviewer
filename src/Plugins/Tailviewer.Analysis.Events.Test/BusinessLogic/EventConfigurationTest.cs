using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Analysis.Events.BusinessLogic;
using Tailviewer.Core;

namespace Tailviewer.Analysis.Events.Test.BusinessLogic
{
	[TestFixture]
	public sealed class EventConfigurationTest
	{
		[Test]
		public void TestRoundtrip()
		{
			var config = new EventConfiguration
			{
				Name = "My custom event",
				FilterExpression = "%d"
			};
			var actualConfig = config.Roundtrip();
			actualConfig.Name.Should().Be("My custom event");
			actualConfig.FilterExpression.Should().Be("%d");
		}
	}
}