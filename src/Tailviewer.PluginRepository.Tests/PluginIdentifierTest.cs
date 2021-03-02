using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Repository;

namespace Tailviewer.PluginRepository.Tests
{
	[TestFixture]
	public sealed class PluginIdentifierTest
	{
		public static IEnumerable<Version> Versions => new[]
		{
			new Version(42, 13),
			new Version(8, 10, 433),
			new Version(1, 5, 27, 7542)
		};

		[Test]
		public void TestRoundtrip([ValueSource(nameof(Versions))] Version version)
		{
			var value = new PluginIdentifier("A", version);
			var actualValue = BinarySerializerTest.Roundtrip(value);
			actualValue.Should().NotBeNull();
			actualValue.Should().NotBeSameAs(value);
			actualValue.Should().Be(value);
		}
	}
}
