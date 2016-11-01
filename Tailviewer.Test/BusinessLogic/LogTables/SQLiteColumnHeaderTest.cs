using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.Test.BusinessLogic.LogTables
{
	[TestFixture]
	public sealed class SQLiteColumnHeaderTest
	{
		[Test]
		public void TestCtor()
		{
			var column = new SQLiteColumnHeader("foo", SQLiteDataType.Other);
			column.Name.Should().Be("foo");
			column.DatabaseType.Should().Be(SQLiteDataType.Other);
		}

		[Test]
		public void TestEquality1()
		{
			new SQLiteColumnHeader("foo", SQLiteDataType.DateTime).Equals(new SQLiteColumnHeader("foo", SQLiteDataType.DateTime))
			                                                      .Should()
			                                                      .BeTrue();
			new SQLiteColumnHeader("foo", SQLiteDataType.DateTime).GetHashCode()
			                                                      .Should()
			                                                      .Be(
				                                                      new SQLiteColumnHeader("foo", SQLiteDataType.DateTime)
					                                                      .GetHashCode());
		}

		[Test]
		[Description("Verifies that a different name results in different column headers")]
		public void TestEquality2()
		{
			new SQLiteColumnHeader("foo", SQLiteDataType.DateTime).Equals(new SQLiteColumnHeader("bar", SQLiteDataType.DateTime))
			                                                      .Should()
			                                                      .BeFalse();
			new SQLiteColumnHeader("foo", SQLiteDataType.DateTime).GetHashCode()
			                                                      .Should()
			                                                      .NotBe(
				                                                      new SQLiteColumnHeader("bar", SQLiteDataType.DateTime)
					                                                      .GetHashCode());
		}

		[Test]
		[Description("Verifies that a different database type results in different column headers")]
		public void TestEquality3()
		{
			new SQLiteColumnHeader("foo", SQLiteDataType.DateTime).Equals(new SQLiteColumnHeader("foo", SQLiteDataType.Integer))
																  .Should()
																  .BeFalse();
			new SQLiteColumnHeader("foo", SQLiteDataType.DateTime).GetHashCode()
																  .Should()
																  .NotBe(
																	  new SQLiteColumnHeader("foo", SQLiteDataType.Integer)
																		  .GetHashCode());
		}
	}
}