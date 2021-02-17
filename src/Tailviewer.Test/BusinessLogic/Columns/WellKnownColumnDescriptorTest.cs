using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Columns;

namespace Tailviewer.Test.BusinessLogic.Columns
{
	[TestFixture]
	public sealed class WellKnownColumnDescriptorTest
	{
		[Test]
		public void TestCustomDefaultValue()
		{
			var column = new WellKnownColumnDescriptor<double>("foo", "Foo", 42);
			column.DefaultValue.Should().Be(42.0);
			((IColumnDescriptor) column).DefaultValue.Should().Be(42.0);
		}

		[Test]
		public void TestConstructionNullable()
		{
			var column = new WellKnownColumnDescriptor<double?>("foo");
			column.Id.Should().Be("foo");
			column.DataType.Should().Be<double?>();
			column.DefaultValue.Should().BeNull();
			((IColumnDescriptor) column).DefaultValue.Should().BeNull();
		}

		[Test]
		public void TestConstructionValueType()
		{
			var column = new WellKnownColumnDescriptor<float>("foo");
			column.Id.Should().Be("foo");
			column.DataType.Should().Be<float>();
			column.DefaultValue.Should().Be(0.0f);
			((IColumnDescriptor)column).DefaultValue.Should().Be(0.0f);
		}

		[Test]
		public void TestConstructionReferenceType()
		{
			var column = new WellKnownColumnDescriptor<string>("foo");
			column.Id.Should().Be("foo");
			column.DataType.Should().Be<string>();
			column.DefaultValue.Should().BeNull();
			((IColumnDescriptor)column).DefaultValue.Should().BeNull();
		}

		[Test]
		public void TestEquality()
		{
			var column = new WellKnownColumnDescriptor<double?>("foo");
			var otherColumn = new CustomColumnDescriptor<double?>("foo");
			column.Equals(otherColumn).Should().BeFalse("because a well known column can never equal a custom column");
		}
		[Test]
		public void TestToString1()
		{
			var column = new WellKnownColumnDescriptor<string>("foobar");
			column.ToString().Should().Be("foobar: String");
		}
	}
}