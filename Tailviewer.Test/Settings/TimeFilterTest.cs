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
		public static IEnumerable<SpecialDateTimeInterval?> Ranges
		{
			get
			{
				return new SpecialDateTimeInterval?[]
				{
					null,
					SpecialDateTimeInterval.Last24Hours,
					SpecialDateTimeInterval.Last30Days,
					SpecialDateTimeInterval.Last7Days,
					SpecialDateTimeInterval.Last365Days,
					SpecialDateTimeInterval.ThisMonth,
					SpecialDateTimeInterval.ThisWeek,
					SpecialDateTimeInterval.ThisYear,
					SpecialDateTimeInterval.Today
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
		public void TestClone([ValueSource(nameof(Ranges))] SpecialDateTimeInterval? range,
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
		public void TestStoreRestore([ValueSource(nameof(Ranges))] SpecialDateTimeInterval? range,
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
