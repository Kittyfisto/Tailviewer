using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Archiver.Tests.Plugins
{
	[TestFixture]
	public sealed class SerializableChangesTest
	{
		[Test]
		public void TestRoundtripEmpty()
		{
			var changes = new SerializableChanges();
			var actualChanges = Roundtrip(changes);
			actualChanges.Should().NotBeNull();
			actualChanges.Should().NotBeSameAs(changes);
			actualChanges.Changes.Should().NotBeNull();
			actualChanges.Changes.Should().BeEmpty();
		}

		[Test]
		public void TestRoundtripOneChange()
		{
			var changes = new SerializableChanges
			{
				Changes =
				{
					new SerializableChange
					{
						Summary = "I made a booboo"
					}
				}
			};
			var actualChanges = Roundtrip(changes);
			actualChanges.Changes.Should().HaveCount(1);
			actualChanges.Changes[0].Summary.Should().Be("I made a booboo");
			actualChanges.Changes[0].Description.Should().BeNullOrEmpty();
		}

		[Test]
		public void TestRoundtripOneTwoChanges()
		{
			var changes = new SerializableChanges
			{
				Changes =
				{
					new SerializableChange
					{
						Summary = "When the fire nation attacked...",
						Description = "Everything changed"
					},
					new SerializableChange
					{
						Summary = "My cabbages",
					}
				}
			};
			var actualChanges = Roundtrip(changes);
			actualChanges.Changes.Should().HaveCount(2);
			actualChanges.Changes[0].Summary.Should().Be("When the fire nation attacked...");
			actualChanges.Changes[0].Description.Should().Be("Everything changed");
			
			actualChanges.Changes[1].Summary.Should().Be("My cabbages");
			actualChanges.Changes[1].Description.Should().BeNullOrEmpty();
		}

		private static SerializableChanges Roundtrip(SerializableChanges changes)
		{
			using (var stream = new MemoryStream())
			{
				changes.Serialize(stream);

				stream.Position = 0;
				return SerializableChanges.Deserialize(stream);
			}
		}
	}
}
