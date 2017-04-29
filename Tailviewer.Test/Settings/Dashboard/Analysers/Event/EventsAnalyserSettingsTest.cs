using System;
using System.IO;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;
using Tailviewer.Settings.Dashboard.Analysers;
using Tailviewer.Settings.Dashboard.Analysers.Event;

namespace Tailviewer.Test.Settings.Dashboard.Analysers.Event
{
	[TestFixture]
	public sealed class EventsAnalyserSettingsTest
	{
		[Test]
		public void TestRoundtrip()
		{
			using (var data = new MemoryStream())
			{
				var settings = new EventsAnalyserSettings
				{
					MaxEvents = 9999,
					Events =
					{
						new EventSettings
						{
							Name = "My custom event",
							FilterType = QuickFilterMatchType.RegexpFilter,
							FilterExpression = "%d",
							IgnoreCase = true,
							DisplayExpression = "A wild number appeared!"
						}
					}
				};

				using (var writer = XmlWriter.Create(data, new XmlWriterSettings
				{
					NewLineHandling = NewLineHandling.Entitize,
					Indent = true,
					NewLineChars = "\r\n"
				}))
				{
					writer.WriteStartElement("Test");
					settings.Save(writer);
					writer.WriteEndElement();
				}
				data.Position = 0;
				using (var reader = new StreamReader(data))
				{
					Console.Write(reader.ReadToEnd());
					data.Position = 0;
					using (var xmlReader = XmlReader.Create(data))
					{
						xmlReader.MoveToContent();
						xmlReader.MoveToElement();
						var actualSettings = (EventsAnalyserSettings)AnalyserSettings.Restore(xmlReader);
						actualSettings.MaxEvents.Should().Be(9999);
						actualSettings.Events.Count.Should().Be(1);
						actualSettings.Events[0].Name.Should().Be("My custom event");
						actualSettings.Events[0].FilterType.Should().Be(QuickFilterMatchType.RegexpFilter);
						actualSettings.Events[0].FilterExpression.Should().Be("%d");
						actualSettings.Events[0].IgnoreCase.Should().BeTrue();
						actualSettings.Events[0].DisplayExpression.Should().Be("A wild number appeared!");
					}
				}
			}
		}
	}
}