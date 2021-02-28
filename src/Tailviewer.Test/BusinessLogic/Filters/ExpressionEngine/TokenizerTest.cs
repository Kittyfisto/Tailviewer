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

		[Test]
		[Description("Verifies that the tokenizer does not split singular word literals just because the word consists partially of another token type such as 'Or'")]
		public void TestParseLogLevelLiteral()
		{
			var tokenizer = new Tokenizer();
			tokenizer.Tokenize("error").Should().Equal(new Token(TokenType.Literal, "error"));
		}

		[Test]
		public void TestParseSimpleExpression()
		{
			var tokenizer = new Tokenizer();
			tokenizer.Tokenize("$loglevel is fatal").Should().Equal(new Token(TokenType.Dollar),
			                                                        new Token(TokenType.Literal, "loglevel"),
			                                                        new Token(TokenType.Whitespace, " "),
			                                                        new Token(TokenType.Is),
			                                                        new Token(TokenType.Whitespace, " "),
			                                                        new Token(TokenType.Literal, "fatal"));
		}
	}
}