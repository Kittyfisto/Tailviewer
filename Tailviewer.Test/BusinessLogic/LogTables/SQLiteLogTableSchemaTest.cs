using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.Test.BusinessLogic.LogTables
{
	[TestFixture]
	public sealed class SQLiteLogTableSchemaTest
	{
		[Test]
		public void TestCtor1()
		{
			var schema = new SQLiteLogTableSchema("log");
			schema.TableName.Should().Be("log");
			schema.ColumnHeaders.Should().NotBeNull();
			schema.ColumnHeaders.Should().BeEmpty();
		}
	}
}