using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;

namespace Tailviewer.Tests.BusinessLogic.Sources.Text
{
	[TestFixture]
	public sealed class LineOffsetDetectorTest
	{
		private MemoryStream _stream;

		public static IReadOnlyList<Encoding> Encodings
		{
			get
			{
				return new[]
				{
					Encoding.Default,
					Encoding.ASCII,
					Encoding.BigEndianUnicode,
					Encoding.UTF32,
					Encoding.UTF8,
					Encoding.UTF7
				};
			}
		}

		[SetUp]
		public void Setup()
		{
			_stream = new MemoryStream();
		}

		[Test]
		public void TestEmpty([ValueSource(nameof(Encodings))] Encoding encoding)
		{
			var detector = new LineOffsetDetector(_stream, encoding);
			detector.FindNextLineOffset().Should().Be(-1);
		}

		[Test]
		public void TestOneCharacter([ValueSource(nameof(Encodings))] Encoding encoding)
		{
			var writer = new StreamWriter(_stream, encoding);
			var line = "A";
			writer.Write(line);
			writer.Flush();
			_stream.Position = 0;

			var detector = new LineOffsetDetector(_stream, encoding);

			detector.FindNextLineOffset().Should().Be(-1, "because we're at the end and there's no newlines");

			detector.FindNextLineOffset().Should().Be(-1, "because we're at the end and there's no newlines");
		}

		[Test]
		public void TestOneNewline([ValueSource(nameof(Encodings))] Encoding encoding)
		{
			var writer = new StreamWriter(_stream, encoding);
			var line = "\n";
			writer.Write(line);
			writer.Flush();
			_stream.Position = 0;

			var preamble = encoding.GetPreamble();
			var detector = new LineOffsetDetector(_stream, encoding);

			var offset = detector.FindNextLineOffset();
			offset.Should().Be(preamble.Length + encoding.GetByteCount(line));
			
			detector.FindNextLineOffset().Should().Be(-1, "because we're at the end and there's no newlines anymore");
		}

		[Test]
		public void TestSentenceWithNewline([ValueSource(nameof(Encodings))] Encoding encoding)
		{
			var writer = new StreamWriter(_stream, encoding);
			var line = "This is a lazy sentence\n";
			writer.Write(line);
			writer.Flush();
			_stream.Position = 0;

			var preamble = encoding.GetPreamble();
			var detector = new LineOffsetDetector(_stream, encoding);
			var offset = detector.FindNextLineOffset();
			offset.Should().Be(preamble.Length + encoding.GetByteCount(line));
			
			detector.FindNextLineOffset().Should().Be(-1, "because we're at the end and there's no newlines anymore");
		}

		[Test]
		public void TestThreeLines([ValueSource(nameof(Encodings))] Encoding encoding,
		                           [Values(1, 2, 4, 8, 16)] int bufferSize)
		{
			var writer = new StreamWriter(_stream, encoding, bufferSize);
			var line1 = "This\n";
			var line2 = "is\n";
			var line3 = "crazy";
			writer.Write(line1);
			writer.Write(line2);
			writer.Write(line3);
			writer.Flush();
			_stream.Position = 0;

			var preamble = encoding.GetPreamble();
			var detector = new LineOffsetDetector(_stream, encoding);
			var offset1 = detector.FindNextLineOffset();
			offset1.Should().Be(preamble.Length + encoding.GetByteCount(line1));

			var offset2 = detector.FindNextLineOffset();
			offset2.Should().Be(offset1 + encoding.GetByteCount(line2));
			
			detector.FindNextLineOffset().Should().Be(-1, "because we're at the end and there's no newlines anymore");
		}
	}
}
