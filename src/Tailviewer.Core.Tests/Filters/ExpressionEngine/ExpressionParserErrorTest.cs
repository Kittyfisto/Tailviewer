using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Filters.ExpressionEngine;

namespace Tailviewer.Core.Tests.Filters.ExpressionEngine
{
	[TestFixture]
	public sealed class ExpressionParserErrorTest
	{
		[Test]
		public void Test1LessThanTrue()
		{
			FailParse("1 < true", "Expected right hand side of '1 < true' to evaluate to a number");
		}

		[Test]
		public void TestMessageGreaterThan42()
		{
			FailParse("$message > 42", "Expected left hand side of '$message > 42' to evaluate to a number");
		}

		[Test]
		[Ignore("Not yet implemented")]
		public void TestTrueAndFoobar()
		{
			FailParse("true and \"foobar\"", "Expected left hand side of '$message > 42' to evaluate to a number");
		}

		private void FailParse(string expression, string innerErrorMessage)
		{
			var completeErrorMessage = string.Format("Unable to parse \"{0}\": {1}", expression, innerErrorMessage);
			new Action(() => { new ExpressionParser().Parse(expression); }).Should().Throw<ParseException>()
				  .WithMessage(completeErrorMessage);
		}
	}
}