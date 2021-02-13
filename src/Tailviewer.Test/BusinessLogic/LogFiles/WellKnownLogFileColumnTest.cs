using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class WellKnownLogFileColumnTest
	{
		[Test]
		public void TestCustomDefaultValue()
		{
			var column = new WellKnownLogFileColumnDescriptor<double>("foo", "Foo", 42);
			column.DefaultValue.Should().Be(42.0);
			((ILogFileColumnDescriptor) column).DefaultValue.Should().Be(42.0);
		}

		[Test]
		public void TestConstructionNullable()
		{
			var column = new WellKnownLogFileColumnDescriptor<double?>("foo");
			column.Id.Should().Be("foo");
			column.DataType.Should().Be<double?>();
			column.DefaultValue.Should().BeNull();
			((ILogFileColumnDescriptor) column).DefaultValue.Should().BeNull();
		}

		[Test]
		public void TestConstructionValueType()
		{
			var column = new WellKnownLogFileColumnDescriptor<float>("foo");
			column.Id.Should().Be("foo");
			column.DataType.Should().Be<float>();
			column.DefaultValue.Should().Be(0.0f);
			((ILogFileColumnDescriptor)column).DefaultValue.Should().Be(0.0f);
		}

		[Test]
		public void TestConstructionReferenceType()
		{
			var column = new WellKnownLogFileColumnDescriptor<string>("foo");
			column.Id.Should().Be("foo");
			column.DataType.Should().Be<string>();
			column.DefaultValue.Should().BeNull();
			((ILogFileColumnDescriptor)column).DefaultValue.Should().BeNull();
		}

		[Test]
		public void TestEquality()
		{
			var column = new WellKnownLogFileColumnDescriptor<double?>("foo");
			var otherColumn = new CustomLogFileColumnDescriptor<double?>("foo");
			column.Equals(otherColumn).Should().BeFalse("because a well known column can never equal a custom column");
		}
	}
}