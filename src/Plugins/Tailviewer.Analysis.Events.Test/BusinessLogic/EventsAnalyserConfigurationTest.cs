using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Tailviewer.Analysis.Events.BusinessLogic;

namespace Tailviewer.Analysis.Events.Test.BusinessLogic
{
	[TestFixture]
	public sealed class EventsAnalyserConfigurationTest
	{
		[Test]
		public void TestRoundtrip()
		{
			using (var data = new MemoryStream())
			{
				var settings = new EventsLogAnalyserConfiguration
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

				using (var writer = XmlWriter.Create(data, new XmlWriterSettings
				{
					NewLineHandling = NewLineHandling.Entitize,
					Indent = true,
					NewLineChars = "\r\n"
				}))
				{
					writer.WriteStartElement("Test");
					//settings.Save(writer);
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

						//var actualSettings = (EventsLogAnalyserConfiguration)LogAnalyserConfiguration.Restore(xmlReader);
						//actualSettings.MaxEvents.Should().Be(9999);
						//actualSettings.Events.Count.Should().Be(1);
						//actualSettings.Events[0].Name.Should().Be("My custom event");
						//actualSettings.Events[0].FilterExpression.Should().Be("%d");
					}
				}
			}
		}
	}
}