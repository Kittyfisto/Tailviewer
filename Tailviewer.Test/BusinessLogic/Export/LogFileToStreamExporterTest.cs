using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Exporter;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.Export
{
	[TestFixture]
	public sealed class LogFileToStreamExporterTest
	{
		[Test]
		public void TestConstruction1()
		{
			new Action(() => new LogFileToStreamExporter(null, new MemoryStream()))
				.ShouldThrow<ArgumentNullException>();
			new Action(() => new LogFileToStreamExporter(new Mock<ILogFile>().Object, null))
				.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestExportEmpty()
		{
			var stream = new MemoryStream();
			var logFile = new InMemoryLogFile();
			var exporter = new LogFileToStreamExporter(logFile, stream);
			new Action(() => exporter.Export()).ShouldNotThrow();
			GetString(stream).Should().Be(string.Empty, "because an empty log file should result in an empty export");
		}

		[Test]
		public void TestExportOneLine()
		{
			var stream = new MemoryStream();
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hello, World!", LevelFlags.None);
			var exporter = new LogFileToStreamExporter(logFile, stream);
			new Action(() => exporter.Export()).ShouldNotThrow();

			GetString(stream).Should().Be("Hello, World!");
		}

		[Test]
		public void TestTwoLines()
		{
			var stream = new MemoryStream();
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hello,", LevelFlags.None);
			logFile.AddEntry("World!", LevelFlags.None);
			var exporter = new LogFileToStreamExporter(logFile, stream);
			new Action(() => exporter.Export()).ShouldNotThrow();

			GetString(stream).Should().Be("Hello,\r\nWorld!");
		}

		private static string GetString(MemoryStream stream)
		{
			stream.Position = 0;
			using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
			{
				var value = reader.ReadToEnd();
				return value;
			}
		}
	}
}