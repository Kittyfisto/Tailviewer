using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.Sources.Text.Simple;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class StreamReaderExTest
	{
		private MemoryStream _stream;
		private StreamWriter _writer;

		[SetUp]
		public void Setup()
		{
			_stream = new MemoryStream();
			_writer = new StreamWriter(_stream);
		}

		[Test]
		public void TestReadLine1()
		{
			var reader = new StreamReaderEx(_stream, Encoding.UTF8);
			reader.ReadLine().Should().BeNull("because the source stream is empty");
		}

		[Test]
		public void TestReadLine2()
		{
			_writer.Write("Foo");
			_writer.Flush();
			_stream.Position = 0;

			var reader = new StreamReaderEx(_stream, Encoding.UTF8);
			reader.ReadLine().Should().Be("Foo");
		}

		[Test]
		public void TestReadLine3()
		{
			_writer.Write("Foo\n");
			_writer.Flush();
			_stream.Position = 0;

			var reader = new StreamReaderEx(_stream, Encoding.UTF8);
			reader.ReadLine().Should().Be("Foo\n");
		}

		[Test]
		public void TestReadLine4()
		{
			_writer.Write("Foo\r\n");
			_writer.Flush();
			_stream.Position = 0;

			var reader = new StreamReaderEx(_stream, Encoding.UTF8);
			reader.ReadLine().Should().Be("Foo\r\n");
		}

		[Test]
		public void TestReadLine5()
		{
			_writer.Write("Foo\r\n");
			_writer.Write("Bar");
			_writer.Flush();
			_stream.Position = 0;

			var reader = new StreamReaderEx(_stream, Encoding.UTF8);
			reader.ReadLine().Should().Be("Foo\r\n");
			reader.ReadLine().Should().Be("Bar");
		}

		[Test]
		public void TestReadLine6()
		{
			var builder = new StringBuilder();
			builder.Append('a', 2049);
			var line = builder.ToString();

			_writer.Write(line);
			_writer.Flush();
			_stream.Position = 0;

			var reader = new StreamReaderEx(_stream, Encoding.UTF8);
			reader.ReadLine().Should().Be(line);
			reader.ReadLine().Should().Be(null);
		}

		[Test]
		public void TestReadLine7()
		{
			var builder = new StringBuilder();
			builder.Append('a', 2049);
			builder.Append("\r\n");
			var line1 = builder.ToString();

			_writer.Write(line1);
			_writer.Write("Foobar");
			_writer.Flush();
			_stream.Position = 0;

			var reader = new StreamReaderEx(_stream, Encoding.UTF8);
			reader.ReadLine().Should().Be(line1);
			reader.ReadLine().Should().Be("Foobar");
			reader.ReadLine().Should().BeNull();
		}

		[Test]
		public void TestReadLine8()
		{
			_writer.Write("Foo\r\n");
			_writer.Write("Bar\r\n");
			_writer.Write("Shazam\r\n");
			_writer.Flush();
			_stream.Position = 0;

			var reader = new StreamReaderEx(_stream, Encoding.UTF8);
			reader.ReadLine().Should().Be("Foo\r\n");
			reader.ReadLine().Should().Be("Bar\r\n");
			reader.ReadLine().Should().Be("Shazam\r\n");
		}
	}
}
