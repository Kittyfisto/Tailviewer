using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Sources.Text;

namespace Tailviewer.Tests.BusinessLogic.Sources.Text
{
	[TestFixture]
	public sealed class EncodingDetectorTest
	{
		public static IReadOnlyList<Encoding> EncodingsWithPreamble = new[]
		{
			Encoding.UTF8,
			Encoding.UTF32,
			Encoding.Unicode,
			Encoding.BigEndianUnicode
		};

		[Test]
		public void TestDetect([ValueSource(nameof(EncodingsWithPreamble))] Encoding encoding)
		{
			var preamble = encoding.GetPreamble();
			preamble.Length.Should().BeGreaterOrEqualTo(1, "because this test doesn't make much sense if the encoding has no preamble");

			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream, encoding))
			{
				writer.Write("Controversial opinion: White House Down is a better movie than Olympus has Fallen, ikr");
				writer.Flush();

				stream.Position = 0;
				var detector = new EncodingDetector();
				detector.TryFindEncoding(stream).Should().Be(encoding);
			}
		}

		[Test]
		public void TestDetectNoPreamble()
		{
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream, Encoding.ASCII))
			{
				writer.Write("Controversial opinion: White House Down is a better movie than Olympus has Fallen, ikr");
				writer.Flush();

				stream.Position = 0;
				var detector = new EncodingDetector();
				detector.TryFindEncoding(stream).Should().Be(null, "because there's no preamble to detect ASCII and the detector shall not make any guesses and give up");
			}
		}
	}
}
