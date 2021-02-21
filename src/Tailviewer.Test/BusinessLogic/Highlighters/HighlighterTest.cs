using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Highlighters;
using Tailviewer.Core.Settings;

namespace Tailviewer.Test.BusinessLogic.Highlighters
{
	[TestFixture]
	public sealed class HighlighterTest
	{
		[Test]
		public void TestCtor()
		{
			var highlighter = new Highlighter();
			highlighter.Id.Should().NotBe(HighlighterId.Empty);
			highlighter.Value.Should().BeNullOrEmpty();
			highlighter.MatchType.Should().Be(FilterMatchType.SubstringFilter);
		}
	}
}
