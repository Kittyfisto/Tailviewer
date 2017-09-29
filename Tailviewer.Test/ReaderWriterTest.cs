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
			: ISerializableType
		{
			public void Serialize(IWriter writer)
			{}

			public void Deserialize(IReader reader)
			{}
		}

		sealed class SomeType
			: ISerializableType
		{
			private string _value;

			public string Value { get { return _value; } set { _value = value; } }

			public void Serialize(IWriter writer)
			{
				writer.WriteAttribute("Value", Value);
			}

			public void Deserialize(IReader reader)
			{
				reader.TryReadAttribute("Value", out _value);
			}
		}

		[Test]
		public void TestEmpty()
		{
			var reader = CloseAndRead();

			reader.FormatVersion.Should().Be(Writer.FormatVersion);
			reader.Created.Should().Be(_writer.Created);
			reader.TailviewerVersion.Should().Be(Core.Constants.ApplicationVersion);
			reader.TailviewerBuildDate.Should().Be(Core.Constants.BuildDate);
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
		public void TestLongAttribute()
		{
			_writer.WriteAttribute("Foo", long.MaxValue);
			_writer.WriteAttribute("Hello", long.MinValue);

			var reader = CloseAndRead();

			long value;
			reader.TryReadAttribute("Hello", out value).Should().BeTrue();
			value.Should().Be(long.MinValue);

			reader.TryReadAttribute("Foo", out value).Should().BeTrue();
			value.Should().Be(long.MaxValue);
		}

		[Test]
		public void TestDateTimeAttribute1([Values(DateTimeKind.Local, DateTimeKind.Unspecified, DateTimeKind.Utc)] DateTimeKind kind)
		{
			var time = new DateTime(2017, 09, 28, 23, 16, 30, kind);
			_writer.WriteAttribute("Foo", time);

			var reader = CloseAndRead();

			DateTime value;
			reader.TryReadAttribute("Foo", out value).Should().BeTrue();
			value.Year.Should().Be(2017);
			value.Month.Should().Be(09);
			value.Day.Should().Be(28);
			value.Hour.Should().Be(23);
			value.Minute.Should().Be(16);
			value.Second.Should().Be(30);
			value.Kind.Should().Be(kind);
		}

		[Test]
		public void TestVersionAttribute1()
		{
			_writer.WriteAttribute("Foo", (Version) null);

			var reader = CloseAndRead();

			Version actualVersion;
			reader.TryReadAttribute("Foo", out actualVersion).Should().BeTrue();
			actualVersion.Should().BeNull();
		}

		[Test]
		public void TestVersionAttribute2([Values("1.0", "2.1", "2.31.2", "10.5.1", "5.85.1.992")] string value)
		{
			var version = Version.Parse(value);
			_writer.WriteAttribute("Foo", version);

			var reader = CloseAndRead();

			Version actualVersion;
			reader.TryReadAttribute("Foo", out actualVersion).Should().BeTrue();
			actualVersion.Should().Be(version);
		}

		[Test]
		public void TestGuidAttribute()
		{
			var id = Guid.NewGuid();
			_writer.WriteAttribute("Foo", id);

			var reader = CloseAndRead();

			Guid value;
			reader.TryReadAttribute("Foo", out value).Should().BeTrue();
			value.Should().Be(id);
		}

		[Test]
		public void TestChild1()
		{
			_writer.WriteAttribute("Stuff", (ISerializableType)null);
			var reader = CloseAndRead();

			ISerializableType value;
			reader.TryReadAttribute("Stuff", out value).Should().BeTrue();
			value.Should().BeNull();
		}

		[Test]
		public void TestChild2()
		{
			_writer.WriteAttribute("Stuff", new EmptyType());
			var reader = CloseAndRead();

			ISerializableType value;
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
		public void TestChild4()
		{
			_writer.WriteAttribute("Stuff", new SomeType {Value = "Important Stuff!"});
			var reader = CloseAndRead();

			var child = new SomeType();
			reader.TryReadAttribute("Stuff", child).Should().BeTrue();
			child.Value.Should().Be("Important Stuff!");
		}

		[Test]
		public void TestChild5()
		{
			_writer.WriteAttribute("Stuff", new SomeType { Value = "Important Stuff!" });
			_writer.WriteAttribute("MyNumber", "0190 666666");
			var reader = CloseAndRead();

			var child = new SomeType();
			reader.TryReadAttribute("Stuff", child).Should().BeTrue();
			child.Value.Should().Be("Important Stuff!");

			string number;
			reader.TryReadAttribute("MyNumber", out number).Should().BeTrue();
			number.Should().Be("0190 666666");
		}

		[Test]
		public void TestChildren1()
		{
			_writer.WriteAttribute("Stuff", new ISerializableType[]
			{
				null,
				null
			});
			var reader = CloseAndRead();

			IEnumerable<ISerializableType> value;
			reader.TryReadAttribute("Stuff", out value).Should().BeTrue();
			value.Should().NotBeNull();
			value.Should().Equal(null, null);
		}

		[Test]
		public void TestChildren2()
		{
			_writer.WriteAttribute("Stuff", new ISerializableType[]
			{
				null,
				new EmptyType()
			});
			var reader = CloseAndRead();

			IEnumerable<ISerializableType> value;
			reader.TryReadAttribute("Stuff", out value).Should().BeTrue();
			value.Should().NotBeNull();
			value.Should().HaveCount(2);
			value.ElementAt(0).Should().BeNull();
			value.ElementAt(1).Should().BeOfType<EmptyType>();
		}

		private Reader CloseAndRead()
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