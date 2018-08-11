using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Filters.ExpressionEngine;

namespace Tailviewer.Test.BusinessLogic.Filters.ExpressionEngine
{
	[TestFixture]
	public sealed class TokenizerTest
	{
		[Test]
		public void TestParseQuotation()
		{
			var tokenizer = new Tokenizer();
			tokenizer.Tokenize("\"").Should().Equal(new Token(TokenType.Quotation));
		}
	}
}