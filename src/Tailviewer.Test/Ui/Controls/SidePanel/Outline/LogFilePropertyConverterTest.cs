using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Formats;
using Tailviewer.Ui.Controls.SidePanel.Outline;

namespace Tailviewer.Test.Ui.Controls.SidePanel.Outline
{
	[TestFixture]
	public sealed class LogFilePropertyConverterTest
	{
		[Test]
		public void TestConvertFileFormatDescriptionNull()
		{
			var format = new TextLogFileFormat("Ultralog", null, null);
			var converter = new LogFilePropertyConverter();
			converter.Convert(format, null, null, null).Should()
			         .Be("Ultralog", "because the converter shall fall back to the name if the description is empty");
		}

		[Test]
		public void TestConvertFileFormatDescriptionEmpty()
		{
			var format = new TextLogFileFormat("Foo", "", null);
			var converter = new LogFilePropertyConverter();
			converter.Convert(format, null, null, null).Should()
			         .Be(format.Name, "because the converter shall fall back to the name if the description is empty");
		}

		[Test]
		public void TestConvertFileFormatDescriptionWhitespace()
		{
			var format = new TextLogFileFormat("Bar", " \t ", null);
			var converter = new LogFilePropertyConverter();
			converter.Convert(format, null, null, null).Should()
			         .Be(format.Name, "because the converter shall fall back to the name if the description is empty");
		}

		[Test]
		public void TestConvertFileFormat()
		{
			var format = new TextLogFileFormat("Ultralog", "The best format in the world", null);
			var converter = new LogFilePropertyConverter();
			converter.Convert(format, null, null, null).Should()
			         .Be(format.Description, "because this format finally offers a description and thus that should have been presented");
		}
	}
}
