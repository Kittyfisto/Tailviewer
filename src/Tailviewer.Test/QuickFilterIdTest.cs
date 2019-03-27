using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;
#pragma warning disable CS1718

namespace Tailviewer.Test
{
	[TestFixture]
	public sealed class QuickFilterIdTest
		: AbstractIdTest<QuickFilterId>
	{
		protected override QuickFilterId Empty()
		{
			return QuickFilterId.Empty;
		}

		protected override QuickFilterId CreateNew()
		{
			return QuickFilterId.CreateNew();
		}

		[Test]
		public void TestCtor()
		{
			var guid = Guid.NewGuid();
			var id = new WidgetId(guid);
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
			var id = QuickFilterId.CreateNew();
			var actualId = Roundtrip(id);
			actualId.Should().NotBeNull();
			actualId.Should().NotBeSameAs(id);
			actualId.Value.Should().Be(id.Value);
		}

		private QuickFilterId Roundtrip(QuickFilterId id)
		{
			using (var stream = new MemoryStream())
			{
				var typeFactory = new TypeFactory();
				using (var writer = new Writer(stream, typeFactory))
				{
					id.Serialize(writer);
				}

				stream.Position = 0;

				var reader = new Reader(stream, typeFactory);
				var actualId = new QuickFilterId();
				actualId.Deserialize(reader);
				return actualId;
			}
		}
	}
}