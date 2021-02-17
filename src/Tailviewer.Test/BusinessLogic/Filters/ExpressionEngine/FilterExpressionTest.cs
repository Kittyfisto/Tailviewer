using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Entries;
using Tailviewer.Core.Filters;

namespace Tailviewer.Test.BusinessLogic.Filters.ExpressionEngine
{
	[TestFixture]
	public sealed class FilterExpressionTest
	{
		[Test]
		public void TestLineNumberGreaterThan5000()
		{
			var expression = FilterExpression.Parse("$linenumber > 5000");
			expression.PassesFilter(new LogEntry(LogColumns.Minimum){LineNumber = 4999})
			          .Should().BeFalse();
			expression.PassesFilter(new LogEntry(LogColumns.Minimum){LineNumber = 5000})
			          .Should().BeFalse();
			expression.PassesFilter(new LogEntry(LogColumns.Minimum){LineNumber = 5001})
			          .Should().BeTrue();
		}
	}
}
