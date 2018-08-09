using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
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
			expression.PassesFilter(new LogLine(4999, 4999, LogLineSourceId.Default, "Stuff", LevelFlags.All,
			                                    DateTime.MinValue))
			          .Should().BeFalse();
			expression.PassesFilter(new LogLine(5000, 5000, LogLineSourceId.Default, "Stuff", LevelFlags.All,
			                                    DateTime.MinValue))
			          .Should().BeTrue();
		}
	}
}
