using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;

namespace Tailviewer.Test
{
	[TestFixture]
	public sealed class ReaderWriterTest
	{
		private MemoryStream _stream;
		private Writer _writer;
		private TypeFactory _typeFactory;

		[SetUp]
		public void Setup()
		{
			_stream = new MemoryStream();
			_writer = new Writer(_stream);

			_typeFactory = new TypeFactory(new []
			{
				new KeyValuePair<string, Type>(typeof(EmptyType).FullName, typeof(EmptyType))
			});
		}

		sealed class EmptyType
			: ISerializable
		{
			public void Serialize(IWriter writer)
			{}

			public void Deserialize(IReader reader)
			{}
		}

		[Test]
		[Description("Verifies that writing/reading a single attribute")]
		public void TestStringAttribute1()
		{
			_writer.WriteAttribute("Foo", "Bar!");

			var reader = CloseAndRead();

			string value;
			reader.TryReadAttribute("Foo", out value).Should().BeTrue();
			value.Should().Be("Bar!");
		}

		[Test]
		[Description("Verifies that writing/reading multiple attributes in order works")]
		public void TestStringAttribute2()
		{
			_writer.WriteAttribute("Foo", "Bar!");
			_writer.WriteAttribute("Hello", "World!");

			var reader = CloseAndRead();

			string value;
			reader.TryReadAttribute("Foo", out value).Should().BeTrue();
			value.Should().Be("Bar!");

			reader.TryReadAttribute("Hello", out value).Should().BeTrue();
			value.Should().Be("World!");
		}

		[Test]
		[Description("Verifies that writing/reading multiple attributes out of order works")]
		public void TestStringAttribute3()
		{
			_writer.WriteAttribute("Foo", "Bar!");
			_writer.WriteAttribute("Hello", "World!");

			var reader = CloseAndRead();

			string value;
			reader.TryReadAttribute("Hello", out value).Should().BeTrue();
			value.Should().Be("World!");

			reader.TryReadAttribute("Foo", out value).Should().BeTrue();
			value.Should().Be("Bar!");
		}

		[Test]
		[Description("Verifies that reading still works, even if we tried to access a non-existing attribute first")]
		public void TestStringAttribute4()
		{
			_writer.WriteAttribute("Foo", "Bar!");

			var reader = CloseAndRead();

			string value;
			reader.TryReadAttribute("Hello", out value).Should().BeFalse();
			value.Should().BeNull();

			reader.TryReadAttribute("Foo", out value).Should().BeTrue();
			value.Should().Be("Bar!");
		}

		[Test]
		[Description("Verifies that writing/reading a single attribute")]
		public void TestIntAttribute1()
		{
			_writer.WriteAttribute("Foo", int.MaxValue);

			var reader = CloseAndRead();

			int value;
			reader.TryReadAttribute("Foo", out value).Should().BeTrue();
			value.Should().Be(int.MaxValue);
		}

		[Test]
		[Description("Verifies that writing/reading multiple attributes in order works")]
		public void TestIntAttribute2()
		{
			_writer.WriteAttribute("Foo", 42);
			_writer.WriteAttribute("Hello", int.MinValue);

			var reader = CloseAndRead();

			int value;
			reader.TryReadAttribute("Foo", out value).Should().BeTrue();
			value.Should().Be(42);

			reader.TryReadAttribute("Hello", out value).Should().BeTrue();
			value.Should().Be(int.MinValue);
		}

		[Test]
		[Description("Verifies that writing/reading multiple attributes out of order works")]
		public void TestIntAttribute3()
		{
			_writer.WriteAttribute("Foo", 9001);
			_writer.WriteAttribute("Hello", -9001);

			var reader = CloseAndRead();

			int value;
			reader.TryReadAttribute("Hello", out value).Should().BeTrue();
			value.Should().Be(-9001);

			reader.TryReadAttribute("Foo", out value).Should().BeTrue();
			value.Should().Be(9001);
		}

		[Test]
		[Description("Verifies that reading still works, even if we tried to access a non-existing attribute first")]
		public void TestIntAttribute4()
		{
			_writer.WriteAttribute("Foo", 42);

			var reader = CloseAndRead();

			int value;
			reader.TryReadAttribute("Hello", out value).Should().BeFalse();
			value.Should().Be(0);

			reader.TryReadAttribute("Foo", out value).Should().BeTrue();
			value.Should().Be(42);
		}

		[Test]
		public void TestChild1()
		{
			_writer.WriteAttribute("Stuff", (ISerializable)null);
			var reader = CloseAndRead();

			ISerializable value;
			reader.TryReadAttribute("Stuff", out value).Should().BeTrue();
			value.Should().BeNull();
		}

		[Test]
		public void TestChild2()
		{
			_writer.WriteAttribute("Stuff", new EmptyType());
			var reader = CloseAndRead();

			ISerializable value;
			reader.TryReadAttribute("Stuff", out value).Should().BeTrue();
			value.Should().BeOfType<EmptyType>();
		}

		[Test]
		public void TestChild3()
		{
			_writer.WriteAttribute("Stuff", new EmptyType());
			var reader = CloseAndRead();

			EmptyType value;
			reader.TryReadAttribute("Stuff", out value).Should().BeTrue();
			value.Should().NotBeNull();
		}

		[Test]
		public void TestChildren1()
		{
			_writer.WriteAttribute("Stuff", new ISerializable[]
			{
				null,
				null
			});
			var reader = CloseAndRead();

			IEnumerable<ISerializable> value;
			reader.TryReadAttribute("Stuff", out value).Should().BeTrue();
			value.Should().NotBeNull();
			value.Should().Equal(null, null);
		}

		[Test]
		public void TestChildren2()
		{
			_writer.WriteAttribute("Stuff", new ISerializable[]
			{
				null,
				new EmptyType()
			});
			var reader = CloseAndRead();

			IEnumerable<ISerializable> value;
			reader.TryReadAttribute("Stuff", out value).Should().BeTrue();
			value.Should().NotBeNull();
			value.Should().HaveCount(2);
			value.ElementAt(0).Should().BeNull();
			value.ElementAt(1).Should().BeOfType<EmptyType>();
		}

		private IReader CloseAndRead()
		{
			_writer.Dispose();
			_stream.Position = 0;

			using (var tmp = new StreamReader(_stream, Encoding.UTF8, true, 4096, true))
			{
				var document = tmp.ReadToEnd();
				Console.WriteLine(document);
			}

			_stream.Position = 0;
			var reader = new Reader(_stream, _typeFactory);
			return reader;
		}
	}
}