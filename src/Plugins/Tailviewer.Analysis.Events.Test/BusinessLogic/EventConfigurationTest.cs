using System.IO;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Events.BusinessLogic;

namespace Tailviewer.Events.Test.BusinessLogic
{
	[TestFixture]
	public sealed class EventConfigurationTest
	{
		[Test]
		public void TestRoundtrip()
		{
			using (var data = new MemoryStream())
			{
				var @event = new EventConfiguration
				{
					Name = "My custom event",
					FilterExpression = "%d"
				};
				using (var writer = XmlWriter.Create(data))
				{
					writer.WriteStartElement("Test");
					@event.Save(writer);
					writer.WriteEndElement();
				}
				data.Position = 0;
				using (var reader = XmlReader.Create(data))
				{
					var actualEvent = new EventConfiguration();
					reader.MoveToContent();
					actualEvent.Restore(reader);
					actualEvent.Name.Should().Be("My custom event");
					actualEvent.FilterExpression.Should().Be("%d");
				}
			}
		}
	}
}