﻿using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.Columns;

namespace Tailviewer.Tests.BusinessLogic.Filters.ExpressionEngine
{
	[TestFixture]
	public sealed class FilterExpressionTest
	{
		[Test]
		public void TestLineNumberGreaterThan5000()
		{
			var expression = FilterExpression.Parse("$linenumber > 5000");
			expression.PassesFilter(new LogEntry(GeneralColumns.Minimum){LineNumber = 4999})
			          .Should().BeFalse();
			expression.PassesFilter(new LogEntry(GeneralColumns.Minimum){LineNumber = 5000})
			          .Should().BeFalse();
			expression.PassesFilter(new LogEntry(GeneralColumns.Minimum){LineNumber = 5001})
			          .Should().BeTrue();
		}

		[Test]
		public void TestToString()
		{
			var expression = FilterExpression.Parse("$linenumber > 5000");
			expression.ToString().Should().Be("$linenumber > 5000");
		}
	}
}
