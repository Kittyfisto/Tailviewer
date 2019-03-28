using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.Settings;

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
			_typeFactory = new TypeFactory();
			_typeFactory.Add("EmptyType", typeof(EmptyType));
			_typeFactory.Add("SomeType", typeof(SomeType));

			_stream = new MemoryStream();
			_writer = new Writer(_stream, _typeFactory);
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
		public void TestBoolAttribute1([Values(true, false)] bool value)
		{
			_writer.WriteAttribute("Foo", value);

			var reader = CloseAndRead();

			bool actualValue;
			reader.TryReadAttribute("Foo", out actualValue).Should().BeTrue();
			actualValue.Should().Be(value);
		}

		[Test]
		[Description("Verifies that writing/reading multiple attributes in order works")]
		public void TestBoolAttribute2([Values(true, false)] bool value1, [Values(true, false)] bool value2)
		{
			_writer.WriteAttribute("Foo", value1);
			_writer.WriteAttribute("Hello", value2);

			var reader = CloseAndRead();

			bool actualValue;
			reader.TryReadAttribute("Foo", out actualValue).Should().BeTrue();
			actualValue.Should().Be(value1);

			reader.TryReadAttribute("Hello", out actualValue).Should().BeTrue();
			actualValue.Should().Be(value2);
		}

		[Test]
		[Description("Verifies that writing/reading multiple attributes out of order works")]
		public void TestBoolAttribute3([Values(true, false)] bool value1, [Values(true, false)] bool value2)
		{
			_writer.WriteAttribute("Foo", value1);
			_writer.WriteAttribute("Hello", value2);

			var reader = CloseAndRead();

			bool actualValue;
			reader.TryReadAttribute("Hello", out actualValue).Should().BeTrue();
			actualValue.Should().Be(value2);

			reader.TryReadAttribute("Foo", out actualValue).Should().BeTrue();
			actualValue.Should().Be(value1);
		}

		[Test]
		[Description("Verifies that reading still works, even if we tried to access a non-existing attribute first")]
		public void TestBoolAttribute4([Values(true, false)] bool value)
		{
			_writer.WriteAttribute("Foo", value);

			var reader = CloseAndRead();

			bool actualValue;
			reader.TryReadAttribute("Hello", out actualValue).Should().BeFalse();
			actualValue.Should().BeFalse();

			reader.TryReadAttribute("Foo", out actualValue).Should().BeTrue();
			actualValue.Should().Be(value);
		}

		[Test]
		public void TestWriteNullableDateTimeAttribute1()
		{
			var value = DateTime.Now;
			_writer.WriteAttribute("Foo", (DateTime?)value);

			var reader = CloseAndRead();

			DateTime? actualValue;
			reader.TryReadAttribute("Foo", out actualValue).Should().BeTrue();
			actualValue.Should().Be(value);
		}

		[Test]
		public void TestWriteNullableDateTimeAttribute2()
		{
			var value = DateTime.Now;
			_writer.WriteAttribute("Foo", (DateTime?)null);

			var reader = CloseAndRead();

			DateTime? actualValue;
			reader.TryReadAttribute("Foo", out actualValue).Should().BeFalse();
			actualValue.Should().BeNull();
		}

		[Test]
		public void TestWriteNullableDateTimeAttribute3()
		{
			_writer.WriteAttribute("Bar", (DateTime?)DateTime.Now);

			var reader = CloseAndRead();

			DateTime? actualValue;
			reader.TryReadAttribute("Foo", out actualValue).Should().BeFalse();
			actualValue.Should().BeNull();
		}

		[Test]
		public void TestWriteNullableDateTimeAttribute4()
		{
			_writer.WriteAttribute("Foo", "dwawdaw");

			var reader = CloseAndRead();

			DateTime? actualValue;
			reader.TryReadAttribute("Foo", out actualValue).Should().BeFalse();
			actualValue.Should().BeNull();
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
		public void TestWriteNullableLongAttribute1()
		{
			var value = 49081240891208912L;
			_writer.WriteAttribute("Foo", (long?)value);

			var reader = CloseAndRead();

			long? actualValue;
			reader.TryReadAttribute("Foo", out actualValue).Should().BeTrue();
			actualValue.Should().Be(value);
		}

		[Test]
		public void TestWriteNullableLongAttribute2()
		{
			_writer.WriteAttribute("Foo", (long?)null);

			var reader = CloseAndRead();

			long? actualValue;
			reader.TryReadAttribute("Foo", out actualValue).Should().BeFalse();
			actualValue.Should().BeNull();
		}

		[Test]
		public void TestWriteNullableLongAttribute3()
		{
			_writer.WriteAttribute("Bar", (long?)49081240891208912L);

			var reader = CloseAndRead();

			long? actualValue;
			reader.TryReadAttribute("Foo", out actualValue).Should().BeFalse();
			actualValue.Should().BeNull();
		}

		[Test]
		public void TestWriteNullableLongAttribute4()
		{
			_writer.WriteAttribute("Foo", "dwawdaw");

			var reader = CloseAndRead();

			long? actualValue;
			reader.TryReadAttribute("Foo", out actualValue).Should().BeFalse();
			actualValue.Should().BeNull();
		}

		[Test]
		public void TestEnumAttribute()
		{
			_writer.WriteAttributeEnum("Foo", FilterMatchType.RegexpFilter);
			_writer.WriteAttributeEnum("Hello", FilterMatchType.WildcardFilter);

			var reader = CloseAndRead();

			FilterMatchType value;
			reader.TryReadAttributeEnum("Hello", out value).Should().BeTrue();
			value.Should().Be(FilterMatchType.WildcardFilter);

			reader.TryReadAttributeEnum("Foo", out value).Should().BeTrue();
			value.Should().Be(FilterMatchType.RegexpFilter);
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