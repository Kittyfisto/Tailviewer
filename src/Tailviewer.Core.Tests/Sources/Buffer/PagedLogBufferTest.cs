using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Sources.Buffer
{
	[TestFixture]
	public sealed class PagedLogBufferTest
	{
		[Test]
		[Description("Verifies that when the cache is empty, then no data can be read from it")]
		public void TestEmpty()
		{
			var buffer = new PagedLogBuffer(1024, 10, Core.Columns.RawContent);

			var destination = new LogBufferArray(12, PageBufferedLogSource.RetrievalState, Core.Columns.RawContent);
			destination[0].RawContent = "Foo";
			destination[1].RawContent = "Bar";
			destination[10].RawContent = "Big Smoke";
			destination[11].RawContent = "Sup";
			buffer.TryGetEntries(new LogSourceSection(0, 10), destination, 1).Should().BeFalse("because the cache is completely empty and thus no data should have been retrieved");

			destination[0].RawContent.Should()
			              .Be("Foo",
			                  "because only the data from the specified offset onward may have been overwritten");
			for (int i = 1; i < 11; ++i)
			{
				destination[i].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotInSource);
				destination[i].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue,
			                                        "because when the region couldn't be read from the cache, then it should have been defaulted");
			}
			destination[11].RawContent.Should().Be("Sup");
		}

		[Test]
		[Description("Verifies that when the cache has been resized, but not filled yet, then it will still not allow reading data from it")]
		public void TestNoData()
		{
			var buffer = new PagedLogBuffer(1024, 10, Core.Columns.RawContent);

			buffer.ResizeTo(12);

			var destination = new LogBufferArray(12, PageBufferedLogSource.RetrievalState, Core.Columns.RawContent);
			destination[0].RawContent = "Foo";
			destination[1].RawContent = "Bar";
			destination[10].RawContent = "Big Smoke";
			destination[11].RawContent = "Sup";
			buffer.TryGetEntries(new LogSourceSection(0, 10), destination, 1).Should().BeFalse("because the cache is completely empty and thus no data should have been retrieved");

			destination[0].RawContent.Should()
			              .Be("Foo",
			                  "because only the data from the specified offset onward may have been overwritten");
			for (int i = 1; i < 11; ++i)
			{
				destination[i].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotCached);
				destination[i].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue,
			                                        "because when the region couldn't be read from the cache, then it should have been defaulted");
			}
			destination[11].RawContent.Should().Be("Sup");
		}

		[Test]
		public void TestRetrieveFromPartiallyFilledPage_CreatedAfterResize()
		{
			var buffer = new PagedLogBuffer(10, 1, Core.Columns.RawContent);

			buffer.ResizeTo(5);
			var data = new LogBufferList(Core.Columns.RawContent)
			{
				new LogEntry{RawContent = "A"},
				new LogEntry{RawContent = "B"}
			};
			buffer.TryAdd(new LogSourceSection(0, 2), data, 0);

			var destination = new LogBufferArray(7, PageBufferedLogSource.RetrievalState, Core.Columns.RawContent);
			buffer.TryGetEntries(new LogSourceSection(0, 7), destination, 0).Should().BeFalse("because the cache is only partially filled");

			destination[0].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.Retrieved);
			destination[1].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.Retrieved);
			destination[2].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotCached);
			destination[3].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotCached);
			destination[4].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotCached);
			destination[5].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotInSource);
			destination[6].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotInSource);
		}

		[Test]
		public void TestRetrieveFromPartiallyFilledPage_CreatedBeforeResize()
		{
			var buffer = new PagedLogBuffer(10, 1, Core.Columns.RawContent);

			buffer.ResizeTo(2);
			var data = new LogBufferList(Core.Columns.RawContent)
			{
				new LogEntry{RawContent = "A"},
				new LogEntry{RawContent = "B"}
			};
			buffer.TryAdd(new LogSourceSection(0, 2), data, 0);
			buffer.ResizeTo(4);

			var destination = new LogBufferArray(6, PageBufferedLogSource.RetrievalState, Core.Columns.RawContent);
			buffer.TryGetEntries(new LogSourceSection(0, 6), destination, 0).Should().BeFalse("because the cache is only partially filled");

			destination[0].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.Retrieved);
			destination[1].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.Retrieved);
			destination[2].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotCached);
			destination[3].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotCached);
			destination[4].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotInSource);
			destination[5].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotInSource);
		}

		[Test]
		[Description("Verifies that when the cache contains the entire region requested, then it allows reading the data back")]
		public void TestRequestFullyCached_Contiguous()
		{
			var buffer = new PagedLogBuffer(1024, 10, Core.Columns.RawContent);

			buffer.ResizeTo(12);

			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent)
			{
				new LogEntry {Index = 2, RawContent = "Scrambled Data",},
				new LogEntry {Index = 3, RawContent = "This is awesome"},
				new LogEntry {Index = 4, RawContent = "Yup"},
				new LogEntry {Index = 5, RawContent = "Twist!"},
				new LogEntry {Index = 10, RawContent = "I shouldn't be cached"}
			};
			buffer.TryAdd(new LogSourceSection(3, 3), data, 1);

			var destination = new LogBufferArray(5, Core.Columns.Index, Core.Columns.RawContent);
			destination[0].Index = 32;
			destination[0].RawContent = "Foo";
			destination[1].Index = 42;
			destination[1].RawContent = "Bar";
			destination[4].Index = 9001;
			destination[4].RawContent = "His power level is over 9000!";
			buffer.TryGetEntries(new LogSourceSection(3, 2), destination, 2).Should().BeTrue();
			destination[0].Index.Should().Be(32, "because we specified an offset and the data before it should not have been overwritten");
			destination[0].RawContent.Should().Be("Foo", "because we specified an offset and the data before it should not have been overwritten");
			destination[1].Index.Should().Be(42, "because we specified an offset and the data before it should not have been overwritten");
			destination[1].RawContent.Should().Be("Bar", "because we specified an offset and the data before it should not have been overwritten");

			destination[2].Index.Should().Be(data[1].Index);
			destination[2].RawContent.Should().Be(data[1].RawContent);
			destination[3].Index.Should().Be(data[2].Index);
			destination[3].RawContent.Should().Be(data[2].RawContent);

			destination[4].Index.Should().Be(9001, "because the data after the requested region shouldn't have been overwritten");
			destination[4].RawContent.Should().Be("His power level is over 9000!", "because the data after the requested region shouldn't have been overwritten");
		}

		[Test]
		[Description("Verifies that the cache can read data from segmented regions of one page of the buffer")]
		public void TestRequestFullyCached_Segmented_ReadFromOnePage()
		{
			var buffer = new PagedLogBuffer(20, 10, Core.Columns.RawContent);

			buffer.ResizeTo(12);

			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent)
			{
				new LogEntry(),
				new LogEntry(),
				new LogEntry(),
				new LogEntry {RawContent = "Ellie",},
				new LogEntry(),
				new LogEntry(),
				new LogEntry(),
				new LogEntry(),
				new LogEntry(),
				new LogEntry(),
				new LogEntry(),
				new LogEntry {RawContent = "Abby"},
				new LogEntry()
			};
			buffer.TryAdd(new LogSourceSection(0, 12), data, 1);

			var destination = new LogBufferArray(3, Core.Columns.Index, Core.Columns.RawContent);
			buffer.TryGetEntries(new []{new LogLineIndex(10), new LogLineIndex(2)}, destination, 1).Should().BeTrue("because all requested data is part of the cache");

			destination[0].Index.Should().Be(Core.Columns.Index.DefaultValue);
			destination[0].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue);
			destination[1].Index.Should().Be(10);
			destination[1].RawContent.Should().Be("Abby");
			destination[2].Index.Should().Be(2);
			destination[2].RawContent.Should().Be("Ellie");
		}

		[Test]
		[Description("Verifies that the cache can read data from segmented regions of two pages of the buffer")]
		public void TestRequestFullyCached_Segmented_ReadFromTwoPages()
		{
			var buffer = new PagedLogBuffer(8, 10, Core.Columns.RawContent);

			buffer.ResizeTo(12);

			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent)
			{
				new LogEntry(),
				new LogEntry(),
				new LogEntry(),
				new LogEntry(),
				new LogEntry(),
				new LogEntry(),
				new LogEntry(),
				new LogEntry(),
				new LogEntry {RawContent = "Dina",},
				new LogEntry {RawContent = "Jesse"}
			};
			buffer.TryAdd(new LogSourceSection(0, 9), data, 1);

			var destination = new LogBufferArray(4, Core.Columns.Index, Core.Columns.RawContent);
			buffer.TryGetEntries(new []{new LogLineIndex(7), new LogLineIndex(8)}, destination, 2).Should().BeTrue("because all requested data is part of the cache");
			
			destination[0].Index.Should().Be(Core.Columns.Index.DefaultValue);
			destination[0].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue);
			destination[1].Index.Should().Be(Core.Columns.Index.DefaultValue);
			destination[1].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue);
			destination[2].Index.Should().Be(7);
			destination[2].RawContent.Should().Be("Dina");
			destination[3].Index.Should().Be(8);
			destination[3].RawContent.Should().Be("Jesse");
		}

		[Test]
		[Description("Verifies that the cache can read data from segmented regions of multiple pages of the buffer where each page is only partially filled")]
		public void TestRequestFullyCached_Contiguous_ReadFromMultipleHalfFiledPages()
		{
			var buffer = new PagedLogBuffer(1024, 2, Core.Columns.RawContent);

			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent);
			for (int i = 512; i < 512 + 1024; ++i)
				data.Add(new LogEntry{Index = i, RawContent = i.ToString()});
			buffer.ResizeTo(512 + data.Count);
			buffer.TryAdd(new LogSourceSection(512, data.Count), data, 0);

			var destination = new LogBufferArray(1024, Core.Columns.Index, Core.Columns.RawContent);
			var readOffset = 512;
			buffer.TryGetEntries(new LogSourceSection(readOffset, destination.Count), destination, 0).Should().BeTrue("because all requested data is part of the cache");

			for (int i = 0; i < destination.Count; ++i)
			{
				var logEntry = destination[i];
				logEntry.Index.Should().Be(readOffset + i);
				logEntry.RawContent.Should().Be((readOffset+i).ToString());
			}
		}

		[Test]
		[Description("Verifies that the cache can read data from segmented regions of multiple pages of the buffer")]
		public void TestRequestFullyCached_Contiguous_ReadFromMultiplePages()
		{
			var buffer = new PagedLogBuffer(1024, 10, Core.Columns.RawContent);

			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent);
			for (int i = 0; i < 5 * 1024; ++i)
				data.Add(new LogEntry{Index = i, RawContent = i.ToString()});
			buffer.ResizeTo(data.Count);
			buffer.TryAdd(new LogSourceSection(0, data.Count), data, 0);

			var destination = new LogBufferArray(2048, Core.Columns.Index, Core.Columns.RawContent);
			var readOffset = 1024;
			buffer.TryGetEntries(new LogSourceSection(readOffset, destination.Count), destination, 0).Should().BeTrue("because all requested data is part of the cache");

			for (int i = 0; i < destination.Count; ++i)
			{
				var logEntry = destination[i];
				logEntry.Index.Should().Be(readOffset+i);
				logEntry.RawContent.Should().Be((readOffset+i).ToString());
			}
		}

		[Test]
		[Description("Verifies that the cache allows partial retrieval of the data and fills the rest with default values")]
		public void TestRequestPartiallyCached_Segmented_ReadFromTwoPages()
		{
			var buffer = new PagedLogBuffer(1024, 10, Core.Columns.RawContent);

			buffer.ResizeTo(12);

			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent)
			{
				new LogEntry(),
				new LogEntry {RawContent = "Dina",},
				new LogEntry {RawContent = "Jesse"}
			};
			buffer.TryAdd(new LogSourceSection(7, 2), data, 1);

			var destination = new LogBufferArray(11, Core.Columns.Index, Core.Columns.RawContent);
			buffer.TryGetEntries(new LogSourceSection(4, 5), destination, 6).Should().BeFalse("because we managed to only retrieve the data partially");


			for (int i = 6; i < 9; ++i)
			{
				destination[i].Index.Should().Be(Core.Columns.Index.DefaultValue);
				destination[i].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue);
			}

			destination[9].Index.Should().Be(7);
			destination[9].RawContent.Should().Be("Dina");
			destination[10].Index.Should().Be(8);
			destination[10].RawContent.Should().Be("Jesse");
		}

		[Test]
		public void TestTryGetEntries_DestinationNull()
		{
			var buffer = new PagedLogBuffer(4, 2, Core.Columns.RawContent);

			buffer.ResizeTo(4);
			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent)
			{
				new LogEntry {RawContent = "A"},
				new LogEntry {RawContent = "B"},
				new LogEntry {RawContent = "C"},
				new LogEntry {RawContent = "D"}
			};
			buffer.TryAdd(new LogSourceSection(0, 8), data, 0);

			new Action(() => buffer.TryGetEntries(new LogSourceSection(0, 2), null, 1))
				.Should().Throw<ArgumentNullException>("because the buffer is too small");
		}

		[Test]
		[Description("Verifies that TryGetEntries checks boundaries and doesn't partially overwrite data in case the boundaries don't match")]
		public void TestTryGetEntries_NegativeDestinationIndex()
		{
			var buffer = new PagedLogBuffer(4, 2, Core.Columns.RawContent);

			buffer.ResizeTo(4);
			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent)
			{
				new LogEntry {RawContent = "A"},
				new LogEntry {RawContent = "B"},
				new LogEntry {RawContent = "C"},
				new LogEntry {RawContent = "D"}
			};
			buffer.TryAdd(new LogSourceSection(0, 8), data, 0);

			var destination = new LogBufferArray(2, Core.Columns.Index, Core.Columns.RawContent);
			destination[0].RawContent = "1";
			destination[1].RawContent = "2";
			new Action(() => buffer.TryGetEntries(new LogSourceSection(0, 2), destination, -1))
				.Should().Throw<ArgumentOutOfRangeException>("because a negative destination index is not allowed");

			destination[0].RawContent.Should().Be("1", "because the original data may not have been overwritten");
			destination[1].RawContent.Should().Be("2", "because the original data may not have been overwritten");
		}

		[Test]
		[Description("Verifies that TryGetEntries checks boundaries and doesn't partially overwrite data in case the boundaries don't match")]
		public void TestTryGetEntries_DestinationTooSmall()
		{
			var buffer = new PagedLogBuffer(4, 2, Core.Columns.RawContent);

			buffer.ResizeTo(4);
			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent)
			{
				new LogEntry {RawContent = "A"},
				new LogEntry {RawContent = "B"},
				new LogEntry {RawContent = "C"},
				new LogEntry {RawContent = "D"}
			};
			buffer.TryAdd(new LogSourceSection(0, 8), data, 0);

			var destination = new LogBufferArray(2, Core.Columns.Index, Core.Columns.RawContent);
			destination[0].RawContent = "1";
			destination[1].RawContent = "2";
			new Action(() => buffer.TryGetEntries(new LogSourceSection(0, 2), destination, 1))
				.Should().Throw<ArgumentException>("because the destination is too small");

			destination[0].RawContent.Should().Be("1", "because the original data may not have been overwritten");
			destination[1].RawContent.Should().Be("2", "because the original data may not have been overwritten");
		}

		[Test]
		[Description("Verifies that the cache adds those portions of data to it which have been previously specified via ResizeTo()")]
		public void TestIgnoreDataOutOfBounds()
		{
			var buffer = new PagedLogBuffer(4, 2, Core.Columns.RawContent);

			buffer.ResizeTo(4);
			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent)
			{
				new LogEntry {RawContent = "A"},
				new LogEntry {RawContent = "B"},
				new LogEntry {RawContent = "C"},
				new LogEntry {RawContent = "D"},
				new LogEntry {RawContent = "E"},
				new LogEntry {RawContent = "F"},
				new LogEntry {RawContent = "G"},
				new LogEntry {RawContent = "H"},
			};
			buffer.TryAdd(new LogSourceSection(0, 8), data, 0);

			var destination = new LogBufferArray(8, Core.Columns.Index, PageBufferedLogSource.RetrievalState, Core.Columns.RawContent);
			buffer.TryGetEntries(new LogSourceSection(0, 8), destination, 0).Should().BeFalse("because even though we've tried to add 8 elements to the buffer, only the first 4 are part of the entire log file section and thus the others should have been ignored");

			destination[0].RawContent.Should().Be("A");
			destination[1].RawContent.Should().Be("B");
			destination[2].RawContent.Should().Be("C");
			destination[3].RawContent.Should().Be("D");

			for (int i = 0; i < 4; ++i)
			{
				destination[i].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.Retrieved);
			}


			for (int i = 4; i < 8; ++i)
			{
				destination[i].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue);
				destination[i].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotInSource);
			}
		}

		[Test]
		[Description("Verifies that the cache doesn't grow endlessly and removes data once the maximum page limit has been hit")]
		public void TestCacheRemovesEntriesWhenNeeded()
		{
			var buffer = new PagedLogBuffer(4, 2, Core.Columns.RawContent);

			buffer.ResizeTo(16);
			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent)
			{
				new LogEntry {RawContent = "A"},
				new LogEntry {RawContent = "B"},
				new LogEntry {RawContent = "C"},
				new LogEntry {RawContent = "D"},
				new LogEntry {RawContent = "E"},
				new LogEntry {RawContent = "F"},
				new LogEntry {RawContent = "G"},
				new LogEntry {RawContent = "H"},
			};
			buffer.TryAdd(new LogSourceSection(0, 8), data, 0);

			var destination = new LogBufferArray(8, Core.Columns.Index, Core.Columns.RawContent, PageBufferedLogSource.RetrievalState);
			buffer.TryGetEntries(new[] {new LogLineIndex(3)}, destination, 0).Should().BeTrue("because the data is still in the cache");
			destination[0].RawContent.Should().Be("D");


			data.Clear();
			data.AddRange(new []{
				new LogEntry {RawContent = "I"},
				new LogEntry {RawContent = "J"},
				new LogEntry {RawContent = "K"},
				new LogEntry {RawContent = "L"},
				new LogEntry {RawContent = "M"},
				new LogEntry {RawContent = "N"},
				new LogEntry {RawContent = "O"},
				new LogEntry {RawContent = "P"},
			});
			//< We deliberately add data that fills the entire cache because we don't want to verify the specific caching algorithm (i.e. which
			// data get's removed), we just want to make sure that it *does* get removed once the cache grows too big.
			buffer.TryAdd(new LogSourceSection(8, 8), data, 0);
			buffer.TryGetEntries(new LogSourceSection(0, 8), destination, 0).Should().BeFalse("because we've added so much data than none of the previously added data may still be part of the cache");
			for (int i = 0; i < destination.Count; ++i)
			{
				destination[i].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue);
				destination[i].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotCached);
			}
		}

		[Test]
		[Description("Verifies that when the source is resized to a smaller size and then resized to a larger one again, once doesn't accidentally read back the invalidated data")]
		public void TestReadInvalidatedRegions()
		{
			var buffer = new PagedLogBuffer(4, 2, Core.Columns.RawContent);

			buffer.ResizeTo(16);
			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent)
			{
				new LogEntry {RawContent = "A"},
				new LogEntry {RawContent = "B"},
				new LogEntry {RawContent = "C"},
				new LogEntry {RawContent = "D"},
				new LogEntry {RawContent = "E"},
				new LogEntry {RawContent = "F"},
				new LogEntry {RawContent = "G"},
				new LogEntry {RawContent = "H"},
			};
			buffer.TryAdd(new LogSourceSection(0, 8), data, 0);

			buffer.ResizeTo(6);

			var destination = new LogBufferArray(8, Core.Columns.Index, Core.Columns.RawContent);
			buffer.TryGetEntries(new LogSourceSection(0, 8), destination, 0).Should().BeFalse("because the data has been partially invalidated");

			destination[0].RawContent.Should().Be("A");
			destination[1].RawContent.Should().Be("B");
			destination[2].RawContent.Should().Be("C");
			destination[3].RawContent.Should().Be("D");
			destination[4].RawContent.Should().Be("E");
			destination[5].RawContent.Should().Be("F");
			destination[6].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue, "because this row has been invalidated when the log source was downsized");
			destination[7].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue, "because this row has been invalidated when the log source was downsized");
		}

		[Test]
		[Description("Verifies that when the source is resized to a smaller size and then resized to a larger one again, once doesn't accidentally read back the invalidated data")]
		public void TestReadPreviouslyInvalidatedButNowAvailableRegions_PartialPageEviction()
		{
			var buffer = new PagedLogBuffer(4, 2, Core.Columns.RawContent);

			buffer.ResizeTo(16);
			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent)
			{
				new LogEntry {RawContent = "A"},
				new LogEntry {RawContent = "B"},
				new LogEntry {RawContent = "C"},
				new LogEntry {RawContent = "D"},
				new LogEntry {RawContent = "E"},
				new LogEntry {RawContent = "F"},
				new LogEntry {RawContent = "G"},
				new LogEntry {RawContent = "H"},
			};
			buffer.TryAdd(new LogSourceSection(0, 8), data, 0);

			buffer.ResizeTo(6);
			buffer.ResizeTo(8);

			var destination = new LogBufferArray(8, Core.Columns.Index, Core.Columns.RawContent);
			buffer.TryGetEntries(new LogSourceSection(0, 8), destination, 0).Should().BeFalse("because the data has been partially invalidated");

			destination[0].RawContent.Should().Be("A");
			destination[1].RawContent.Should().Be("B");
			destination[2].RawContent.Should().Be("C");
			destination[3].RawContent.Should().Be("D");
			destination[4].RawContent.Should().Be("E");
			destination[5].RawContent.Should().Be("F");
			destination[6].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue, "because this row has been invalidated when the log source was downsized");
			destination[7].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue, "because this row has been invalidated when the log source was downsized");
		}

		[Test]
		public void TestReadPreviouslyInvalidatedBigRegion()
		{
			var buffer = new PagedLogBuffer(4, 2, Core.Columns.RawContent);

			buffer.ResizeTo(16);
			var data = new LogBufferList(Core.Columns.Index, Core.Columns.RawContent)
			{
				new LogEntry {RawContent = "A"},
				new LogEntry {RawContent = "B"},
				new LogEntry {RawContent = "C"},
				new LogEntry {RawContent = "D"},
				new LogEntry {RawContent = "E"},
				new LogEntry {RawContent = "F"},
				new LogEntry {RawContent = "G"},
				new LogEntry {RawContent = "H"},
			};
			buffer.TryAdd(new LogSourceSection(0, 8), data, 0);

			buffer.ResizeTo(2); //< Let's invalidate an entire

			var destination = new LogBufferArray(4, Core.Columns.Index, Core.Columns.RawContent);
			buffer.TryGetEntries(new LogSourceSection(4, 4), destination, 0).Should().BeFalse("because the data we're trying to read has been fully invalidated");

			for (int i = 0; i < destination.Count; ++i)
			{
				destination[i].Index.Should().Be(Core.Columns.Index.DefaultValue, "because the data we're trying to read has been fully invalidated");
				destination[i].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue, "because the data we're trying to read has been fully invalidated");
			}
		}

		[Test]
		[Description("Verifies that the cache allows indexing into entries of index 'Invalid' just lke it allows indexing into regions greater than the total number of log entries")]
		public void TestReadPartiallyInvalidIndices()
		{
			var buffer = new PagedLogBuffer(4, 2, Core.Columns.RawContent);

			buffer.ResizeTo(4);
			var data = new LogBufferList(Core.Columns.RawContent)
			{
				new LogEntry {RawContent = "A"},
				new LogEntry {RawContent = "B"},
				new LogEntry {RawContent = "C"},
				new LogEntry {RawContent = "D"},
			};
			buffer.TryAdd(new LogSourceSection(0, 4), data, 0);

			var destination = new LogBufferArray(4, Core.Columns.Index, PageBufferedLogSource.RetrievalState, Core.Columns.RawContent);
			for (int i = 0; i < destination.Count; ++i)
				destination[i].RawContent = i.ToString();
			buffer.TryGetEntries(new[] {new LogLineIndex(0), new LogLineIndex(3), LogLineIndex.Invalid, new LogLineIndex(1)},
			                     destination, 0);

			destination[0].Index.Should().Be(0);
			destination[0].RawContent.Should().Be("A");
			destination[0].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.Retrieved);
			destination[1].Index.Should().Be(3);
			destination[1].RawContent.Should().Be("D");
			destination[1].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.Retrieved);
			destination[2].Index.Should().Be(Core.Columns.Index.DefaultValue, "because we specified an invalid index to be retrieved here");
			destination[2].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue, "because we specified an invalid index to be retrieved here");
			destination[2].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotInSource, "because we specified an invalid index to be retrieved here");
			destination[3].Index.Should().Be(1);
			destination[3].RawContent.Should().Be("B");
			destination[3].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.Retrieved);
		}

		[Test]
		[Description("Verifies that the sets the correct flags when retrieving data from the cache in a non-contiguous fashion")]
		public void TestReadPartiallyUncachedIndices()
		{
			var buffer = new PagedLogBuffer(4, 2, Core.Columns.RawContent);

			buffer.ResizeTo(5);
			var data = new LogBufferList(Core.Columns.RawContent)
			{
				new LogEntry {RawContent = "A"},
				new LogEntry {RawContent = "B"}
			};
			buffer.TryAdd(new LogSourceSection(0, 2), data, 0);

			var destination = new LogBufferArray(5, Core.Columns.Index, PageBufferedLogSource.RetrievalState, Core.Columns.RawContent);
			for (int i = 0; i < destination.Count; ++i)
				destination[i].RawContent = i.ToString();
			buffer.TryGetEntries(new[] {new LogLineIndex(0), new LogLineIndex(3), new LogLineIndex(1), new LogLineIndex(4), new LogLineIndex(5)},
			                     destination, 0);

			destination[0].Index.Should().Be(0);
			destination[0].RawContent.Should().Be("A");
			destination[0].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.Retrieved);

			destination[1].Index.Should().Be(Core.Columns.Index.DefaultValue, "because we tried to retrieve data that isn't cached");
			destination[1].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue, "because we tried to retrieve data that isn't cached");
			destination[1].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotCached, "because we tried to retrieve data that isn't cached");

			destination[2].Index.Should().Be(1);
			destination[2].RawContent.Should().Be("B");
			destination[2].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.Retrieved);

			destination[3].Index.Should().Be(Core.Columns.Index.DefaultValue, "because we tried to retrieve data that isn't cached");
			destination[3].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue, "because we tried to retrieve data that isn't cached");
			destination[3].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotCached, "because we tried to retrieve data that isn't cached");

			destination[4].Index.Should().Be(Core.Columns.Index.DefaultValue, "because we tried to retrieve data from outside the source");
			destination[4].RawContent.Should().Be(Core.Columns.RawContent.DefaultValue, "because we tried to retrieve data from outside the source");
			destination[4].GetValue(PageBufferedLogSource.RetrievalState).Should().Be(RetrievalState.NotInSource, "because we tried to retrieve data from outside the source");
		}
	}
}
