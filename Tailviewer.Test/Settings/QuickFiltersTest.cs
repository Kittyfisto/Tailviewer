using System;
using System.IO;
using System.Text;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.Settings;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class QuickFiltersTest
	{
		[Test]
		public void TestConstruction()
		{
			var filters = new QuickFilters();
			filters.Should().BeEmpty();
			filters.TimeFilter.Should().NotBeNull();
			filters.TimeFilter.Mode.Should().Be(TimeFilterMode.Everything);
			filters.TimeFilter.Minimum.Should().BeNull();
			filters.TimeFilter.Maximum.Should().BeNull();
		}

		[Test]
		public void TestClone()
		{
			var filters = new QuickFilters
			{
				new QuickFilter
				{
					MatchType = FilterMatchType.WildcardFilter
				},
			};
			filters.TimeFilter.SpecialInterval = SpecialDateTimeInterval.Today;
			var clone = filters.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(filters);
			clone.Count.Should().Be(1);
			clone[0].Should().NotBeNull();
			clone[0].Should().NotBeSameAs(filters[0]);
			clone[0].MatchType.Should().Be(filters[0].MatchType);
			clone.TimeFilter.Should().NotBeNull();
			clone.TimeFilter.SpecialInterval.Should().Be(SpecialDateTimeInterval.Today);
		}

		[Test]
		public void TestStoreRestore()
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(stream))
				{
					writer.WriteStartElement("Test");
					var settings = new QuickFilters
					{
						TimeFilter = {SpecialInterval = SpecialDateTimeInterval.ThisWeek}
					};
					settings.Add(new QuickFilter{Value = "42"});
					settings.Save(writer);
					writer.WriteEndElement();
				}

				stream.Position = 0;
				Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));

				using (var reader = XmlReader.Create(stream))
				{
					reader.MoveToContent();

					var settings = new QuickFilters();
					settings.Restore(reader);
					settings.TimeFilter.SpecialInterval.Should().Be(SpecialDateTimeInterval.ThisWeek);
					settings.Should().HaveCount(1);
					settings[0].Value.Should().Be("42");
				}
			}
		}

		[Test]
		public void TestRoundtripEmpty()
		{
			var filters = new QuickFilters();
			var actualFilters = Roundtrip(filters);
			actualFilters.Should().NotBeNull();
			actualFilters.Should().BeEmpty();
		}

		[Test]
		public void TestRoundtripTwoFilters()
		{
			var filters = new QuickFilters();
			filters.Add(new QuickFilter
			{
				IgnoreCase = true,
				IsInverted = false,
				Value = "A",
				MatchType = FilterMatchType.TimeFilter
			});
			filters.Add(new QuickFilter
			{
				IgnoreCase = false,
				IsInverted = true,
				Value = "B",
				MatchType = FilterMatchType.SubstringFilter
			});

			var actualFilters = Roundtrip(filters);
			actualFilters.Should().NotBeNull();
			actualFilters.Should().HaveCount(2);

			actualFilters[0].Id.Should().Be(filters[0].Id);
			actualFilters[0].IgnoreCase.Should().BeTrue();
			actualFilters[0].IsInverted.Should().BeFalse();
			actualFilters[0].Value.Should().Be("A");
			actualFilters[0].MatchType.Should().Be(FilterMatchType.TimeFilter);

			actualFilters[1].Id.Should().Be(filters[1].Id);
			actualFilters[1].IgnoreCase.Should().BeFalse();
			actualFilters[1].IsInverted.Should().BeTrue();
			actualFilters[1].Value.Should().Be("B");
			actualFilters[1].MatchType.Should().Be(FilterMatchType.SubstringFilter);
		}

		private QuickFilters Roundtrip(QuickFilters quickFilters)
		{
			using (var stream = new MemoryStream())
			{
				var typeFactory = CreateTypeFactory();

				using (var writer = new Writer(stream, typeFactory))
				{
					quickFilters.Serialize(writer);
				}

				stream.Position = 0;
				Print(stream);
				stream.Position = 0;

				var reader = new Reader(stream, typeFactory);
				var actualQuickFilters = new QuickFilters();
				actualQuickFilters.Deserialize(reader);
				return actualQuickFilters;
			}
		}

		private static TypeFactory CreateTypeFactory()
		{
			var factory = new TypeFactory();
			factory.Add<QuickFilter>();
			factory.Add<QuickFilterId>();
			return factory;
		}

		private void Print(MemoryStream stream)
		{
			var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true);
			var content = reader.ReadToEnd();
			TestContext.Progress.WriteLine(content);
		}
	}
}