using System;
using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.Sources.Text.Simple;

namespace Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text.Simple
{
	[TestFixture]
	public sealed class StreamReaderExTest
	{
		[Test]
		public void TestReadBigLine1()
		{
			var fileName = TextLogSourceAcceptanceTest.File1Mb_1Line;
			var actualLines = File.ReadAllLines(fileName);
			using (var stream = File.OpenRead(fileName))
			using (var reader = new StreamReaderEx(stream, Encoding.Default))
			{
				var line1 = reader.ReadLine();
				line1.Should().EndWith("\n");
				var trimmedLine1 = line1.TrimEnd();
				trimmedLine1.Equals(actualLines[0]).Should().BeTrue(); //< Using Should().Be(..) crashes VS because it's unable to print 2 Mb
				//line1.TrimEnd().Should().Be(actualLines[0]);

				reader.ReadLine().Should().BeNull("because there's no more lines");
			}
		}

		[Test]
		public void TestReadBigLine2()
		{
			var fileName = TextLogSourceAcceptanceTest.File1Mb_2Lines;
			var actualLines = File.ReadAllLines(fileName);
			using (var stream = File.OpenRead(fileName))
			using (var reader = new StreamReaderEx(stream, Encoding.Default))
			{
				var line1 = reader.ReadLine();
				line1.Should().EndWith("\n");
				var trimmedLine1 = line1.TrimEnd();
				trimmedLine1.Equals(actualLines[0]).Should().BeTrue(); //< Using Should().Be(..) crashes VS because it's unable to print 2 Mb
				//line1.TrimEnd().Should().Be(actualLines[0]);

				var line2 = reader.ReadLine();
				line2.Should().EndWith("\n");
				var trimmedLine2 = line2.Trim();
				trimmedLine2.Equals(actualLines[1]).Should().BeTrue(); //< Using Should().Be(..) crashes VS because it's unable to print 2 Mb
				//line2.TrimEnd().Should().Be(actualLines[1]);

				reader.ReadLine().Should().BeNull("because there's no more lines");
			}
		}

		// This test exists as a reference point in case StreamReaderEx is modified by future me again to be incredibly slow...
		//[Test]
		//[Repeat(10)]
		//public void TestReadAll_FileReadAllText()
		//{
		//	File.ReadAllText(TextLogFileAcceptanceTest.File1Mb_1Line);
		//}

		[Test]
		[Repeat(10)]
		[Description("This test exists to verify that StreamReaderEx isn't becoming super slow again")]
		public void TestReadLine_Performance()
		{
			using (var stream =
				File.OpenRead(TextLogSourceAcceptanceTest.File1Mb_1Line))
			using (var reader = new StreamReaderEx(stream, Encoding.Default))
			{
				reader.ExecutionTimeOf(x => x.ReadLine()).Should().BeLessThan(TimeSpan.FromSeconds(0.5));
			}
		}
	}
}
