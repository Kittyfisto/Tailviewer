using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Buffers
{
	[TestFixture]
	public sealed class LogBufferArrayTest
		: AbstractReadOnlyLogBufferTest
	{
		[Test]
		public void TestConstruction2([Values(1, 2, 5, 10, 42, 100, 9001)] int count)
		{
			var buffer = new LogBufferArray(count);
			buffer.Columns.Should().BeEmpty();
			buffer.Count.Should().Be(count);
			buffer.Should().HaveCount(count);
		}

		[Test]
		public void TestConstruction3([Values(1, 2, 5, 10, 42, 100, 9001)] int count)
		{
			var buffer = new LogBufferArray(count, new List<IColumnDescriptor> {GeneralColumns.RawContent, GeneralColumns.DeltaTime});
			buffer.Columns.Should().Equal(new object[] {GeneralColumns.RawContent, GeneralColumns.DeltaTime });
			buffer.Count.Should().Be(count);
			buffer.Should().HaveCount(count);
		}

		[Test]
		public void TestConstructionNullColumns()
		{
			new Action(() => new LogBufferArray(1, null))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestContainsColumn()
		{
			var buffer = new LogBufferArray(42, new IColumnDescriptor[]{GeneralColumns.LogLevel, GeneralColumns.Index, GeneralColumns.RawContent});
			buffer.Contains(GeneralColumns.LogLevel).Should().BeTrue();
			buffer.Contains(GeneralColumns.Index).Should().BeTrue();
			buffer.Contains(GeneralColumns.RawContent).Should().BeTrue();

			buffer.Contains(GeneralColumns.Message).Should().BeFalse();
			buffer.Contains(GeneralColumns.OriginalLineNumber).Should().BeFalse();
			buffer.Contains(GeneralColumns.DeltaTime).Should().BeFalse();
		}

		[Test]
		public void TestGetEntryByIndex1([Values(-1, 0, 1)] int invalidIndex)
		{
			var buffer = new LogBufferArray(0);
			new Action(() =>
			{
				var unused = buffer[invalidIndex];
			}).Should().Throw<IndexOutOfRangeException>();
		}

		[Test]
		public void TestCopyFrom()
		{
			var buffer = new LogBufferArray(1, GeneralColumns.Timestamp);
			var timestamp = new DateTime(2017, 12, 11, 21, 41, 0);
			buffer.CopyFrom(GeneralColumns.Timestamp, new DateTime?[] {timestamp});
			buffer[0].Timestamp.Should().Be(timestamp, "Because we've just copied this timestamp to the buffer");
		}

		[Test]
		[Description("Verifies that the sourceIndex parameter is honored")]
		public void TestCopyFromPartial1()
		{
			var buffer = new LogBufferArray(1, GeneralColumns.Timestamp);
			var unusedTimestamp = new DateTime(2017, 12, 11, 21, 41, 0);
			var timestamp = new DateTime(2017, 12, 11, 21, 43, 0);
			buffer.CopyFrom(GeneralColumns.Timestamp, 0, new DateTime?[] { unusedTimestamp, timestamp }, 1, 1);
			buffer[0].Timestamp.Should().Be(timestamp, "Because we've just copied this timestamp to the buffer");
		}

		[Test]
		[Description("Verifies that the sourceIndex parameter is honored")]
		public void TestCopyFromPartial2()
		{
			var buffer = new LogBufferArray(1, GeneralColumns.Timestamp);
			var timestamp = new DateTime(2017, 12, 11, 21, 41, 0);
			var unusedTimestamp = new DateTime(2017, 12, 11, 21, 44, 0);
			buffer.CopyFrom(GeneralColumns.Timestamp, 0, new DateTime?[] { timestamp, unusedTimestamp }, 0, 1);
			buffer[0].Timestamp.Should().Be(timestamp, "Because we've just copied this timestamp to the buffer");
		}

		[Test]
		[Description("Verifies that the destIndex parameter is honored")]
		public void TestCopyFromPartial3()
		{
			var buffer = new LogBufferArray(2, GeneralColumns.Timestamp);

			var timestamp = new DateTime(2017, 12, 11, 21, 41, 0);
			buffer.CopyFrom(GeneralColumns.Timestamp, 1, new DateTime?[] { timestamp }, 0, 1);
			buffer[0].Timestamp.Should().BeNull("because we didn't copy any data for this timestamp");
			buffer[1].Timestamp.Should().Be(timestamp, "Because we've just copied this timestamp to the buffer");
		}

		[Test]
		public void TestCopyFromNullColumn()
		{
			var buffer = new LogBufferArray(2, GeneralColumns.Timestamp);
			new Action(() => buffer.CopyFrom(null, 0, new string[0], 0, 0))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestCopyFromUnknownColumn()
		{
			var buffer = new LogBufferArray(2, GeneralColumns.Timestamp);
			new Action(() => buffer.CopyFrom(GeneralColumns.RawContent, 0, new string[0], 0, 0))
				.Should().Throw<NoSuchColumnException>();
		}

		[Test]
		public void TestCopyFromMultipleColumns()
		{
			var buffer = new LogBufferArray(2,
			                                       GeneralColumns.Index,
			                                       GeneralColumns.Timestamp);
			buffer.CopyFrom(GeneralColumns.Index, new LogLineIndex[] {1, 42});
			buffer.CopyFrom(GeneralColumns.Timestamp, new DateTime?[] {DateTime.MinValue, DateTime.MaxValue});

			buffer[0].Index.Should().Be(1);
			buffer[0].Timestamp.Should().Be(DateTime.MinValue);

			buffer[1].Index.Should().Be(42);
			buffer[1].Timestamp.Should().Be(DateTime.MaxValue);
		}

		[Test]
		public void TestCopyFromManyRows()
		{
			const int count = 1000;
			var buffer = new LogBufferArray(count, GeneralColumns.OriginalIndex);
			buffer.CopyFrom(GeneralColumns.OriginalIndex, Enumerable.Range(0, count).Select(i => (LogLineIndex)i).ToArray());
			for (int i = 0; i < count; ++i)
			{
				buffer[i].OriginalIndex.Should().Be(i);
			}
		}

		[Test]
		public void TestCopyFromOverwrite()
		{
			var buffer = new LogBufferArray(2, GeneralColumns.RawContent);
			buffer[0].RawContent.Should().BeNull();
			buffer[1].RawContent.Should().BeNull();

			buffer.CopyFrom(GeneralColumns.RawContent, new [] {"foo", "bar"});
			buffer[0].RawContent.Should().Be("foo");
			buffer[1].RawContent.Should().Be("bar");
		}

		[Test]
		public void TestAccessUnavailableColumn()
		{
			var buffer = new LogBufferArray(1);
			new Action(() => { var unused = buffer[0].RawContent; }).Should().Throw<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].Index; }).Should().Throw<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].OriginalIndex; }).Should().Throw<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].LineNumber; }).Should().Throw<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].OriginalLineNumber; }).Should().Throw<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].Timestamp; }).Should().Throw<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].ElapsedTime; }).Should().Throw<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].DeltaTime; }).Should().Throw<ColumnNotRetrievedException>();
		}

		[Test]
		[Description("Verifies that an entire column can be filled with default values")]
		public void TestFillDefault1([Range(0, 10)] int length)
		{
			var buffer = new LogBufferArray(length, GeneralColumns.RawContent);
			var data = Enumerable.Range(0, length).Select(unused => "Foo").ToArray();
			buffer.CopyFrom(GeneralColumns.RawContent, data);

			for(int i = 0; i < length; ++i)
			{
				buffer[i].RawContent.Should().Be("Foo");
			}

			buffer.FillDefault(GeneralColumns.RawContent, 0, length);

			for (int i = 0; i < length; ++i)
			{
				buffer[i].RawContent.Should().BeNull();
			}
		}

		[Test]
		[Description("Verifies that an entire column can be filled with default values")]
		public void TestFillDefault2([Range(0, 10)] int length)
		{
			var buffer = new LogBufferArray(length, GeneralColumns.Timestamp);
			var data = Enumerable.Range(0, length).Select(unused => (DateTime?)new DateTime(2017, 12, 12, 18, 58, 0)).ToArray();
			buffer.CopyFrom(GeneralColumns.Timestamp, data);

			for (int i = 0; i < length; ++i)
			{
				buffer[i].Timestamp.Should().Be(new DateTime(2017, 12, 12, 18, 58, 0));
			}

			buffer.FillDefault(GeneralColumns.Timestamp, 0, length);

			for (int i = 0; i < length; ++i)
			{
				buffer[i].Timestamp.Should().BeNull();
			}
		}

		[Test]
		[Description("Verifies that a column can be partially filled with default values")]
		public void TestFillDefault3()
		{
			var buffer = new LogBufferArray(4, GeneralColumns.DeltaTime);
			var data = new TimeSpan?[]
			{
				TimeSpan.FromMilliseconds(1),
				TimeSpan.FromMilliseconds(5),
				TimeSpan.FromSeconds(3),
				TimeSpan.FromSeconds(10)
			};
			buffer.CopyFrom(GeneralColumns.DeltaTime, data);
			buffer.FillDefault(GeneralColumns.DeltaTime, 1, 2);

			buffer[0].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));
			buffer[1].DeltaTime.Should().Be(null);
			buffer[2].DeltaTime.Should().Be(null);
			buffer[3].DeltaTime.Should().Be(TimeSpan.FromSeconds(10));
		}

		[Test]
		[Description("Verifies that all columns can be filled with default values")]
		public void TestFillAllColumns1()
		{
			var buffer = new LogBufferArray(4, GeneralColumns.DeltaTime, GeneralColumns.Timestamp);
			var deltas = new TimeSpan?[]
			{
				TimeSpan.FromMilliseconds(1),
				TimeSpan.FromMilliseconds(5),
				TimeSpan.FromSeconds(3),
				TimeSpan.FromSeconds(10)
			};
			buffer.CopyFrom(GeneralColumns.DeltaTime, deltas);
			var timestamps = new DateTime?[]
			{
				new DateTime(2017, 12, 12, 19, 24, 0),
				new DateTime(2017, 12, 12, 19, 25, 0),
				new DateTime(2017, 12, 12, 19, 26, 0),
				new DateTime(2017, 12, 12, 19, 27, 0)
			};
			buffer.CopyFrom(GeneralColumns.Timestamp, timestamps);

			buffer[0].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));
			buffer[0].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 24, 0));
			buffer[1].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(5));
			buffer[1].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 25, 0));
			buffer[2].DeltaTime.Should().Be(TimeSpan.FromSeconds(3));
			buffer[2].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 26, 0));
			buffer[3].DeltaTime.Should().Be(TimeSpan.FromSeconds(10));
			buffer[3].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 27, 0));

			buffer.FillDefault(0, 4);
			buffer[0].DeltaTime.Should().BeNull();
			buffer[0].Timestamp.Should().BeNull();
			buffer[1].DeltaTime.Should().BeNull();
			buffer[1].Timestamp.Should().BeNull();
			buffer[2].DeltaTime.Should().BeNull();
			buffer[2].Timestamp.Should().BeNull();
			buffer[3].DeltaTime.Should().BeNull();
			buffer[3].Timestamp.Should().BeNull();
		}

		[Test]
		[Description("Verifies that all columns can be partially filled with default values")]
		public void TestFillAllColumns2()
		{
			var buffer = new LogBufferArray(4, GeneralColumns.DeltaTime, GeneralColumns.Timestamp);
			var deltas = new TimeSpan?[]
			{
				TimeSpan.FromMilliseconds(1),
				TimeSpan.FromMilliseconds(5),
				TimeSpan.FromSeconds(3),
				TimeSpan.FromSeconds(10)
			};
			buffer.CopyFrom(GeneralColumns.DeltaTime, deltas);
			var timestamps = new DateTime?[]
			{
				new DateTime(2017, 12, 12, 19, 24, 0),
				new DateTime(2017, 12, 12, 19, 25, 0),
				new DateTime(2017, 12, 12, 19, 26, 0),
				new DateTime(2017, 12, 12, 19, 27, 0)
			};
			buffer.CopyFrom(GeneralColumns.Timestamp, timestamps);

			buffer[0].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));
			buffer[0].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 24, 0));
			buffer[1].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(5));
			buffer[1].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 25, 0));
			buffer[2].DeltaTime.Should().Be(TimeSpan.FromSeconds(3));
			buffer[2].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 26, 0));
			buffer[3].DeltaTime.Should().Be(TimeSpan.FromSeconds(10));
			buffer[3].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 27, 0));

			buffer.FillDefault(1, 2);
			buffer[0].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));
			buffer[0].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 24, 0));
			buffer[1].DeltaTime.Should().BeNull();
			buffer[1].Timestamp.Should().BeNull();
			buffer[2].DeltaTime.Should().BeNull();
			buffer[2].Timestamp.Should().BeNull();
			buffer[3].DeltaTime.Should().Be(TimeSpan.FromSeconds(10));
			buffer[3].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 27, 0));
		}

		[Test]
		public void TestColumn()
		{
			var buffer = new LogBufferArray(4, GeneralColumns.DeltaTime, GeneralColumns.Timestamp);
			buffer.Column(GeneralColumns.DeltaTime).Should().Equal(new object[] {null, null, null, null});

			buffer.CopyFrom(GeneralColumns.DeltaTime, 0, new TimeSpan?[]
				{
					TimeSpan.FromDays(1),
					TimeSpan.FromSeconds(42),
					null,
					TimeSpan.FromMinutes(-10)
				},
				0, 4);
			buffer.Column(GeneralColumns.DeltaTime).Should().Equal(new object[]
			{
				TimeSpan.FromDays(1),
				TimeSpan.FromSeconds(42),
				null,
				TimeSpan.FromMinutes(-10)
			});
		}

		[Test]
		public void TestFill()
		{
			var buffer = new LogBufferArray(10, GeneralColumns.LineNumber);
			buffer.Fill(GeneralColumns.LineNumber, 42, 2, 4);
			buffer.Fill(GeneralColumns.LineNumber, 1337, 7, 2);

			buffer[0].LineNumber.Should().Be(GeneralColumns.LineNumber.DefaultValue);
			buffer[1].LineNumber.Should().Be(GeneralColumns.LineNumber.DefaultValue);
			buffer[2].LineNumber.Should().Be(42);
			buffer[3].LineNumber.Should().Be(42);
			buffer[4].LineNumber.Should().Be(42);
			buffer[5].LineNumber.Should().Be(42);
			buffer[6].LineNumber.Should().Be(GeneralColumns.LineNumber.DefaultValue);
			buffer[7].LineNumber.Should().Be(1337);
			buffer[8].LineNumber.Should().Be(1337);
			buffer[9].LineNumber.Should().Be(GeneralColumns.LineNumber.DefaultValue);
		}

		[Test]
		public void TestFill_NullColumn()
		{
			var buffer = new LogBufferArray(10, GeneralColumns.LineNumber);
			new Action(() => buffer.Fill(null, 42, 2, 4)).Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestFillInvalidLength()
		{
			var buffer = new LogBufferArray(10, GeneralColumns.LineNumber);
			buffer.Fill(GeneralColumns.LineNumber, 42, 0, 10);
			new Action(() => buffer.Fill(GeneralColumns.LineNumber, 9001, 0, 11))
				.Should().Throw<ArgumentException>();

			for(int i = 0; i < 10; ++i)
				buffer[i].LineNumber.Should().Be(42, "because nothing may have been overwritten");
		}

		[Test]
		public void TestContainsAny()
		{
			var buffer = new LogBufferArray(3, GeneralColumns.LineNumber);
			buffer[0].LineNumber = 1;
			buffer[1].LineNumber = 2;
			buffer[2].LineNumber = 3;

			buffer.ContainsAny(GeneralColumns.LineNumber, 1, new Int32Range(0, 1)).Should().BeTrue();
			buffer.ContainsAny(GeneralColumns.LineNumber, 1, new Int32Range(0, 2)).Should().BeTrue();
			buffer.ContainsAny(GeneralColumns.LineNumber, 1, new Int32Range(0, 3)).Should().BeTrue();
			
			buffer.ContainsAny(GeneralColumns.LineNumber, 1, new Int32Range(1, 1)).Should().BeFalse();
			buffer.ContainsAny(GeneralColumns.LineNumber, 1, new Int32Range(1, 2)).Should().BeFalse();
			buffer.ContainsAny(GeneralColumns.LineNumber, 1, new Int32Range(2, 1)).Should().BeFalse();

			buffer.ContainsAny(GeneralColumns.LineNumber, 2, new Int32Range(0, 3)).Should().BeTrue();
			buffer.ContainsAny(GeneralColumns.LineNumber, 2, new Int32Range(0, 1)).Should().BeFalse();
			buffer.ContainsAny(GeneralColumns.LineNumber, 2, new Int32Range(2, 1)).Should().BeFalse();
		}

		protected override IReadOnlyLogBuffer CreateEmptyReadOnly(IEnumerable<IColumnDescriptor> columns)
		{
			return new LogBufferArray(0, columns);
		}

		protected override IReadOnlyLogBuffer CreateReadOnly(IEnumerable<IReadOnlyLogEntry> entries)
		{
			if (entries.Any())
			{
				var list = new LogBufferArray(entries.Count(), entries.First().Columns);
				int i = 0;
				foreach (var entry in entries)
				{
					foreach (var column in entry.Columns)
					{
						var value = entry.GetValue(column);
						list[i].SetValue(column, value);
					}
				}
				return list;
			}

			return CreateEmptyReadOnly(new IColumnDescriptor[0]);
		}
	}
}