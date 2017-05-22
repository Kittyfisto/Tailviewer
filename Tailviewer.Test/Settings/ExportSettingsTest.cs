using System;
using System.IO;
using System.Text;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class ExportSettingsTest
	{
		[Test]
		public void TestClone()
		{
			var settings = new ExportSettings
			{
				ExportFolder = "foobar"
			};
			var clone = settings.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(settings);
			clone.ExportFolder.Should().Be("foobar");
		}

		[Test]
		public void TestStoreRestore([Values("", @"C:\foo", "dwawdwadaw")] string folder)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(stream))
				{
					writer.WriteStartElement("Test");
					var settings = new ExportSettings
					{
						ExportFolder = folder
					};
					settings.Save(writer);
					writer.WriteEndElement();
				}

				stream.Position = 0;
				Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));

				using (var reader = XmlReader.Create(stream))
				{
					reader.MoveToContent();

					var settings = new ExportSettings();
					settings.Restore(reader);
					settings.ExportFolder.Should().Be(folder);
				}
			}
		}
	}
}