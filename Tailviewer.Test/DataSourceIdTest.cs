using System;
using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.Test
{
	[TestFixture]
	public sealed class DataSourceIdTest
		: AbstractIdTest<DataSourceId>
	{
		protected override DataSourceId Empty()
		{
			return DataSourceId.Empty;
		}

		protected override DataSourceId CreateNew()
		{
			return DataSourceId.CreateNew();
		}

		[Test]
		public void TestCtor()
		{
			var guid = Guid.NewGuid();
			var id = new WidgetId(guid);
			id.Value.Should().Be(guid);
		}

		[Test]
		public void TestOpEquals()
		{
			var id1 = CreateNew();
			var id2 = CreateNew();
			(id1 == id1).Should().BeTrue();
			(id2 == id2).Should().BeTrue();
			(id1 == id2).Should().BeFalse();
			(id2 == id1).Should().BeFalse();

			(id1 != id1).Should().BeFalse();
			(id2 != id2).Should().BeFalse();
			(id1 != id2).Should().BeTrue();
			(id2 != id1).Should().BeTrue();
		}
	}
}