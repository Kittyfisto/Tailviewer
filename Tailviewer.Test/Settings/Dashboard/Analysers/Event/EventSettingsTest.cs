using System.IO;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;
using Tailviewer.Settings.Dashboard.Analysers.Event;

namespace Tailviewer.Test.Settings.Dashboard.Analysers.Event
{
	[TestFixture]
	public sealed class EventSettingsTest
	{
		[Test]
		public void TestRoundtrip()
		{
			using (var data = new MemoryStream())
			{
				var @event = new EventSettings
				{
					Name = "My custom event",
					FilterType = QuickFilterMatchType.RegexpFilter,
					FilterExpression = "%d",
					IgnoreCase = true,
					DisplayExpression = "A wild number appeared!"
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
					var actualEvent = new EventSettings();
					reader.MoveToContent();
					actualEvent.Restore(reader);
					actualEvent.Name.Should().Be("My custom event");
					actualEvent.FilterType.Should().Be(QuickFilterMatchType.RegexpFilter);
					actualEvent.FilterExpression.Should().Be("%d");
					actualEvent.IgnoreCase.Should().Be(true);
					actualEvent.DisplayExpression.Should().Be("A wild number appeared!");
				}
			}
		}
	}
}