using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Filters.ExpressionEngine;

namespace Tailviewer.Test.BusinessLogic.Filters.ExpressionEngine
{
	[TestFixture]
	public sealed class ExpressionParserTest
	{
		public IExpression Parse(string expression)
		{
			var parser = new ExpressionParser();
			return parser.Parse(expression);
		}

		[Test]
		public void TestParseTrue()
		{
			Parse("true").Should().Be(new BoolLiteral(true));
		}

		[Test]
		public void TestParseFalse()
		{
			Parse("false").Should().Be(new BoolLiteral(false));
		}

		[Test]
		public void TestParseNotTrue()
		{
			Parse("!true").Should().Be(new NotExpression(new BoolLiteral(true)));
		}

		[Test]
		public void TestParseNotFalse()
		{
			Parse("!false").Should().Be(new NotExpression(new BoolLiteral(false)));
		}

		[Test]
		public void TestParseTrueAndFalse()
		{
			Parse("true and false").Should().Be(new AndExpression(new BoolLiteral(true), new BoolLiteral(false)));
		}

		[Test]
		public void TestParseFalseOrTrue()
		{
			Parse("false or true").Should().Be(new OrExpression(new BoolLiteral(false), new BoolLiteral(true)));
		}

		[Test]
		public void TestParseToday()
		{
			Parse("today").Should().Be(new DateTimeIntervalLiteral(DateTimeInterval.Today));
		}

		[Test]
		public void TestParseLineNumber()
		{
			Parse("$linenumber").Should().Be(new LineNumberExpression());
		}

		[Test]
		public void TestParseTimestamp()
		{
			Parse("$timestamp").Should().Be(new TimestampExpression());
		}

		[Test]
		public void TestParseTimestampIsToday()
		{
			Parse("$timestamp is today")
				.Should().Be(new ContainsTimestampExpression(new TimestampExpression(), new DateTimeIntervalLiteral(DateTimeInterval.Today)));
		}

		[Test]
		public void TestParseIntegerLiteral([Values(0, 1, 42, 9001, int.MaxValue)] int value)
		{
			Parse(value.ToString()).Should().Be(new IntegerLiteral(value));
		}

		[Test]
		public void TestParse1LessThan2()
		{
			Parse("1 < 2").Should().Be(new LessThanExpression(new IntegerLiteral(1), new IntegerLiteral(2)));
		}

		[Test]
		public void TestParse5LessOrEquals42()
		{
			Parse("5 <= 42").Should().Be(new LessOrEqualsExpression(new IntegerLiteral(5), new IntegerLiteral(42)));
		}

		[Test]
		public void TestParse1GreaterThan2()
		{
			Parse("1 > 2").Should().Be(new GreaterThanExpression(new IntegerLiteral(1), new IntegerLiteral(2)));
		}

		[Test]
		public void TestParse5GreaterOrEquals42()
		{
			Parse("5 >= 42").Should().Be(new GreaterOrEqualsExpression(new IntegerLiteral(5), new IntegerLiteral(42)));
		}
	}
}