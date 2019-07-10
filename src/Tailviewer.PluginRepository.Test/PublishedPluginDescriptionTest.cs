using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Repository;

namespace Tailviewer.PluginRepository.Test
{
	[TestFixture]
	public sealed class PublishedPluginDescriptionTest
	{
		[Test]
		public void TestRoundtrip()
		{
			var description = new PublishedPluginDescription
			{
				Author = "Simon",
				Publisher = "Dreamcatcher",
				Description = "And there was no one left",
				SizeInBytes = 2131412,
			};

			var actualDescription = BinarySerializerTest.Roundtrip(description);
			actualDescription.Author.Should().Be("Simon");
			actualDescription.Publisher.Should().Be("Dreamcatcher");
			actualDescription.Description.Should().Be("And there was no one left");
			actualDescription.SizeInBytes.Should().Be(2131412);
		}
	}
}