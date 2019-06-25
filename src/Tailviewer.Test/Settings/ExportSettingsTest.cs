using System.IO;
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
		public void TestStoreRestore([Values("A", @"C:\foo", "dwawdwadaw")] string folder)
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
				//Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));

				using (var reader = XmlReader.Create(stream))
				{
					reader.MoveToContent();

					var settings = new ExportSettings();
					settings.Restore(reader);
					settings.ExportFolder.Should().Be(folder);
				}
			}
		}

		[Test]
		[Description("Verifies that upon restoration, the default export folder is used if none could be restored")]
		public void TestRestore1()
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(stream))
				{
					writer.WriteStartElement("Test");
					writer.WriteEndElement();
				}

				stream.Position = 0;
				//Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));

				using (var reader = XmlReader.Create(stream))
				{
					reader.MoveToContent();

					var settings = new ExportSettings();
					settings.Restore(reader);
					settings.ExportFolder.Should().Be(Constants.ExportDirectory,
						"because a sensible default value should be used if the settings are completely empty");
				}
			}
		}
	}
}