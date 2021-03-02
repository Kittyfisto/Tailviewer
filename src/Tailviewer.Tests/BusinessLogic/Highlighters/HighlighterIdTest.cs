using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api.Tests;
using Tailviewer.BusinessLogic.Highlighters;
using Tailviewer.Core;

#pragma warning disable CS1718

namespace Tailviewer.Tests.BusinessLogic.Highlighters
{
	[TestFixture]
	public sealed class HighlighterIdTest
		: AbstractIdTest<HighlighterId>
	{
		protected override HighlighterId Empty()
		{
			return HighlighterId.Empty;
		}

		protected override HighlighterId CreateNew()
		{
			return HighlighterId.CreateNew();
		}

		[Test]
		public void TestCtor()
		{
			var guid = Guid.NewGuid();
			var id = new HighlighterId(guid);
			id.Value.Should().Be(guid);
		}

		[Test]
		public void TestOpEquals()
		{
			var id1 = CreateNew();
			var id2 = CreateNew();
			(id1 == id1).Should().BeTrue();
			(id2 == id2).Should().BeTrue();
			(id1 == id2).Should().BeFalse();
			(id2 == id1).Should().BeFalse();

			(id1 != id1).Should().BeFalse();
			(id2 != id2).Should().BeFalse();
			(id1 != id2).Should().BeTrue();
			(id2 != id1).Should().BeTrue();
		}

		[Test]
		public void TestRoundtrip()
		{
			var id = HighlighterId.CreateNew();
			var actualId = id.Roundtrip();
			actualId.Should().NotBeNull();
			actualId.Should().NotBeSameAs(id);
			actualId.Value.Should().Be(id.Value);
		}
	}
}