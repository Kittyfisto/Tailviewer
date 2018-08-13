using System;
using System.IO;
using System.Text;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
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
			filters.TimeFilter.Range.Should().BeNull();
			filters.TimeFilter.Start.Should().BeNull();
			filters.TimeFilter.End.Should().BeNull();
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
			filters.TimeFilter.Range = SpecialDateTimeInterval.Today;
			var clone = filters.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(filters);
			clone.Count.Should().Be(1);
			clone[0].Should().NotBeNull();
			clone[0].Should().NotBeSameAs(filters[0]);
			clone[0].MatchType.Should().Be(filters[0].MatchType);
			clone.TimeFilter.Should().NotBeNull();
			clone.TimeFilter.Range.Should().Be(SpecialDateTimeInterval.Today);
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
						TimeFilter = {Range = SpecialDateTimeInterval.ThisWeek}
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
					settings.TimeFilter.Range.Should().Be(SpecialDateTimeInterval.ThisWeek);
					settings.Should().HaveCount(1);
					settings[0].Value.Should().Be("42");
				}
			}
		}
	}
}