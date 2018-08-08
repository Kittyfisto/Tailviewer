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
			Parse("true").Should().Be(new TrueExpression());
		}

		[Test]
		public void TestParseFalse()
		{
			Parse("false").Should().Be(new FalseExpression());
		}

		[Test]
		public void TestParseNotTrue()
		{
			Parse("!true").Should().Be(new NotExpression(new TrueExpression()));
		}

		[Test]
		public void TestParseNotFalse()
		{
			Parse("!false").Should().Be(new NotExpression(new FalseExpression()));
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
	}
}