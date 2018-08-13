using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Settings;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class TimeFilterTest
	{
		public static IEnumerable<SpecialTimeRange?> Ranges
		{
			get
			{
				return new SpecialTimeRange?[]
				{
					null,
					SpecialTimeRange.Last24Hours,
					SpecialTimeRange.Last30Days,
					SpecialTimeRange.Last7Days,
					SpecialTimeRange.Last365Days,
					SpecialTimeRange.ThisMonth,
					SpecialTimeRange.ThisWeek,
					SpecialTimeRange.ThisYear,
					SpecialTimeRange.Today
				};
			}
		}

		public static IEnumerable<DateTime?> DateTimes
		{
			get
			{
				return new DateTime?[]
				{
					null,
					new DateTime(2017, 1, 1, 0, 0, 0),
					new DateTime(2018, 6, 30, 12, 39, 58)
				};
			}
		}

		[Test]
		public void TestClone([ValueSource(nameof(Ranges))] SpecialTimeRange? range,
		                      [ValueSource(nameof(DateTimes))] DateTime? start,
		                      [ValueSource(nameof(DateTimes))] DateTime? end)
		{
			var filter = new TimeFilter
			{
				Range = range,
				Start = start,
				End = end
			};

			var clone = filter.Clone();
			clone.Range.Should().Be(range);
			clone.Start.Should().Be(start);
			clone.End.Should().Be(end);
		}

		[Test]
		public void TestStoreRestore([ValueSource(nameof(Ranges))] SpecialTimeRange? range,
		                             [ValueSource(nameof(DateTimes))] DateTime? start,
		                             [ValueSource(nameof(DateTimes))] DateTime? end)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = XmlWriter.Create(stream))
				{
					writer.WriteStartElement("Test");
					var settings = new TimeFilter
					{
						Range = range,
						Start = start,
						End = end
					};
					settings.Save(writer);
					writer.WriteEndElement();
				}

				stream.Position = 0;
				Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));

				using (var reader = XmlReader.Create(stream))
				{
					reader.MoveToContent();

					var settings = new TimeFilter();
					settings.Restore(reader);
					settings.Range.Should().Be(range);
					settings.Start.Should().Be(start);
					settings.End.Should().Be(end);
				}
			}
		}
	}
}
