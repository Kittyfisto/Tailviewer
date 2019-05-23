using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	public abstract class AbstractLogFileTest
	{
		protected abstract ILogFile CreateEmpty();

		/// <summary>
		///     Creates a new log file with the given content.
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		protected abstract ILogFile CreateFromContent(IReadOnlyLogEntries content);

		[Test]
		public void TestDebuggerVisualization1()
		{
			var content = new LogEntryBuffer(2, LogFileColumns.Minimum);
			content[0].Timestamp = new DateTime(2017, 12, 20, 13, 22, 0);
			content[1].Timestamp = new DateTime(2017, 12, 20, 13, 23, 0);
			var logFile = CreateFromContent(content);
			var visualizer = new LogFileView(logFile);
			var logEntries = visualizer.LogEntries;
			logEntries.Should().NotBeNull();
			logEntries.Should().HaveCount(2);
		}

		[Test]
		public void TestStartEndTimestampEmptyLogFile()
		{
			var logFile = CreateEmpty();
			logFile.GetValue(LogFileProperties.StartTimestamp).Should().BeNull();
			logFile.GetValue(LogFileProperties.EndTimestamp).Should().BeNull();
		}

		[Test]
		[Ignore("Not implemented yet")]
		public void TestStartEndTimestamp1()
		{
			var content = new LogEntryList(LogFileColumns.Timestamp);
			content.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 21, 14, 11, 0)});
			var logFile = CreateFromContent(content);
			logFile.GetValue(LogFileProperties.StartTimestamp).Should().Be(new DateTime(2017, 12, 21, 14, 11, 0));
			logFile.GetValue(LogFileProperties.EndTimestamp).Should().Be(new DateTime(2017, 12, 21, 14, 11, 0));
		}

		[Test]
		[Ignore("Not implemented yet")]
		public void TestStartEndTimestamp2()
		{
			var content = new LogEntryList(LogFileColumns.Timestamp);
			content.Add(ReadOnlyLogEntry.Empty);
			content.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 21, 14, 12, 0)});
			content.Add(ReadOnlyLogEntry.Empty);
			content.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 21, 14, 13, 0)});
			content.Add(ReadOnlyLogEntry.Empty);
			var logFile = CreateFromContent(content);
			logFile.GetValue(LogFileProperties.StartTimestamp).Should().Be(new DateTime(2017, 12, 21, 14, 12, 0));
			logFile.GetValue(LogFileProperties.EndTimestamp).Should().Be(new DateTime(2017, 12, 21, 14, 13, 0));
		}

		#region Well Known Columns

		[Test]
		public void TestGetNullColumn()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogFileSection(0, 0), null, new int[0], 0)).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestGetColumnNullIndices()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(null, LogFileColumns.Index, new LogLineIndex[0], 0)).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestGetColumnNullBuffer1()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogFileSection(), LogFileColumns.Index, null, 0)).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestGetColumnNullBuffer2()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogLineIndex[0], LogFileColumns.Index, null, 0)).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestGetColumnBufferTooSmall1()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogFileSection(1, 10),
			                                   LogFileColumns.OriginalLineNumber,
			                                   new int[9])).ShouldThrow<ArgumentException>("because the given buffer is less than the amount of retrieved entries");
		}

		[Test]
		public void TestGetColumnBufferTooSmall2()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogLineIndex[10],
			                                   LogFileColumns.OriginalLineNumber,
			                                   new int[9])).ShouldThrow<ArgumentException>("because the given buffer is less than the amount of retrieved entries");
		}

		[Test]
		public void TestGetColumnDestinationIndexTooSmall1()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogFileSection(1, 10),
			                                   LogFileColumns.OriginalLineNumber,
			                                   new int[10],
			                                   -1)).ShouldThrow<ArgumentOutOfRangeException>("because the given destination index shouldn't be negative");
		}

		[Test]
		public void TestGetColumnDestinationIndexTooSmall2()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogLineIndex[10],
			                                   LogFileColumns.OriginalLineNumber,
			                                   new int[10],
			                                   -1)).ShouldThrow<ArgumentOutOfRangeException>("because the given destination index shouldn't be negative");
		}

		[Test]
		public void TestGetColumnDestinationIndexTooBig1()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogFileSection(1, 10),
			                                   LogFileColumns.OriginalLineNumber,
			                                   new int[15],
			                                   6)).ShouldThrow<ArgumentException>("because the given length and offset are greater than the buffer length");
		}

		[Test]
		public void TestGetColumnDestinationIndexTooBig2()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogLineIndex[10],
			                                   LogFileColumns.OriginalLineNumber,
			                                   new int[15],
			                                   6)).ShouldThrow<ArgumentException>("because the given length and offset are greater than the buffer length");
		}

		#region Index

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetIndexEmptyBySection([Range(from: 0, to: 2)] int count,
											   [Range(from: 0, to: 2)] int offset,
											   [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new LogLineIndex[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = 42;
			}

			logFile.GetColumn(new LogFileSection(0, count), LogFileColumns.Index, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(42, "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().Be(LogLineIndex.Invalid, "because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset+count; i < offset+count+surplus; ++i)
			{
				buffer[i].Should().Be(42, "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetIndexEmptyByIndices([Range(from: -1, to: 1)] int invalidIndex,
		                                       [Range(from: 0, to: 2)] int count,
		                                       [Range(from: 0, to: 2)] int offset,
		                                       [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new LogLineIndex[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = 9001;
			}

			var indices = Enumerable.Range(invalidIndex, count).Select(x => (LogLineIndex)x).ToArray();
			logFile.GetColumn(indices, LogFileColumns.Index, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(9001, "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().Be(LogLineIndex.Invalid, "because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(9001, "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		#endregion

		#region Original Index

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetOriginalIndexEmptyBySection([Range(from: 0, to: 2)] int count,
		                                               [Range(from: 0, to: 2)] int offset,
		                                               [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new LogLineIndex[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = 42;
			}

			logFile.GetColumn(new LogFileSection(0, count), LogFileColumns.OriginalIndex, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(42, "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().Be(LogLineIndex.Invalid, "because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(42, "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetOriginalIndexEmptyByIndices([Range(from: -1, to: 1)] int invalidIndex,
		                                               [Range(from: 0, to: 2)] int count,
		                                               [Range(from: 0, to: 2)] int offset,
		                                               [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new LogLineIndex[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = 9001;
			}

			var indices = Enumerable.Range(invalidIndex, count).Select(x => (LogLineIndex)x).ToArray();
			logFile.GetColumn(indices, LogFileColumns.OriginalIndex, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(9001, "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().Be(LogLineIndex.Invalid, "because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(9001, "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		[Test]
		public void TestGetOriginalIndicesBySection()
		{
			var content = new LogEntryList(LogFileColumns.Timestamp);
			content.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 21, 0, 0, 0)});
			content.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 21, 0, 0, 1)});
			content.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 21, 0, 0, 2)});
			content.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 21, 0, 0, 3)});
			var logFile = CreateFromContent(content);

			var indices = new LogLineIndex[5];
			indices[0] = new LogLineIndex(42);
			indices[4] = new LogLineIndex(9001);

			logFile.GetColumn(new LogFileSection(1, 3), LogFileColumns.OriginalIndex, indices, 1);
			indices[0].Should().Be(42, "because the original value shouldn't have been written over");
			indices[1].Should().Be(1);
			indices[2].Should().Be(2);
			indices[3].Should().Be(3);
			indices[4].Should().Be(9001, "because the original value shouldn't have been written over");
		}

		[Test]
		public void TestGetOriginalIndicesByIndices()
		{
			var content = new LogEntryList(LogFileColumns.Timestamp);
			content.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 21, 0, 0, 0)});
			content.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 21, 0, 0, 1)});
			content.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 21, 0, 0, 2)});
			content.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 21, 0, 0, 3)});
			var logFile = CreateFromContent(content);

			var indices = new LogLineIndex[5];
			indices[0] = new LogLineIndex(42);
			indices[4] = new LogLineIndex(9001);

			logFile.GetColumn(new LogLineIndex[] {3,1,2}, LogFileColumns.OriginalIndex, indices, 1);
			indices[0].Should().Be(42, "because the original value shouldn't have been written over");
			indices[1].Should().Be(3);
			indices[2].Should().Be(1);
			indices[3].Should().Be(2);
			indices[4].Should().Be(9001, "because the original value shouldn't have been written over");
		}

		#endregion

		#region Line Number

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetLineNumberEmptyBySection([Range(from: 0, to: 2)] int count,
		                                            [Range(from: 0, to: 2)] int offset,
		                                            [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new int[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = 42;
			}

			logFile.GetColumn(new LogFileSection(0, count), LogFileColumns.LineNumber, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(42, "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().Be(0, "because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(42, "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetLineNumberEmptyByIndices([Range(from: -1, to: 1)] int invalidIndex,
		                                            [Range(from: 0, to: 2)] int count,
		                                            [Range(from: 0, to: 2)] int offset,
		                                            [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new int[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = 9001;
			}

			var indices = Enumerable.Range(invalidIndex, count).Select(x => (LogLineIndex)x).ToArray();
			logFile.GetColumn(indices, LogFileColumns.LineNumber, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(9001, "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().Be(0, "because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(9001, "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		#endregion

		#region Original Line Number

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetOriginalLineNumberEmptyBySection([Range(from: 0, to: 2)] int count,
		                                                    [Range(from: 0, to: 2)] int offset,
		                                                    [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new int[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = 42;
			}

			logFile.GetColumn(new LogFileSection(0, count), LogFileColumns.OriginalLineNumber, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(42, "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().Be(0, "because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(42, "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetOriginalLineNumberEmptyByIndices([Range(from: -1, to: 1)] int invalidIndex,
		                                                    [Range(from: 0, to: 2)] int count,
		                                                    [Range(from: 0, to: 2)] int offset,
		                                                    [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new int[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = 9001;
			}

			var indices = Enumerable.Range(invalidIndex, count).Select(x => (LogLineIndex)x).ToArray();
			logFile.GetColumn(indices, LogFileColumns.OriginalLineNumber, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(9001, "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().Be(0, "because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(9001, "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		#endregion

		#region Log Level

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetLogLevelEmptyBySection([Range(from: 0, to: 2)] int count,
		                                          [Range(from: 0, to: 2)] int offset,
		                                          [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new LevelFlags[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = LevelFlags.Fatal;
			}

			logFile.GetColumn(new LogFileSection(0, count), LogFileColumns.LogLevel, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(LevelFlags.Fatal, "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().Be(LevelFlags.None, "because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(LevelFlags.Fatal, "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetLogLevelEmptyByIndices([Range(from: -1, to: 1)] int invalidIndex,
		                                          [Range(from: 0, to: 2)] int count,
		                                          [Range(from: 0, to: 2)] int offset,
		                                          [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new LevelFlags[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = LevelFlags.Fatal;
			}

			var indices = Enumerable.Range(invalidIndex, count).Select(x => (LogLineIndex)x).ToArray();
			logFile.GetColumn(indices, LogFileColumns.LogLevel, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(LevelFlags.Fatal, "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().Be(LevelFlags.None, "because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(LevelFlags.Fatal, "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		#endregion

		#region Elapsed Time

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetElapsedTimesEmptyBySection([Range(from: 0, to: 2)] int count,
		                                              [Range(from: 0, to: 2)] int offset,
		                                              [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new TimeSpan?[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = TimeSpan.FromDays(1);
			}

			logFile.GetColumn(new LogFileSection(0, count), LogFileColumns.ElapsedTime, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(TimeSpan.FromDays(1), "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().BeNull("because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(TimeSpan.FromDays(1), "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetElapsedTimesEmptyByIndices([Range(from: -1, to: 1)] int invalidIndex,
		                                              [Range(from: 0, to: 2)] int count,
		                                              [Range(from: 0, to: 2)] int offset,
		                                              [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new TimeSpan?[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = TimeSpan.FromDays(1);
			}

			var indices = Enumerable.Range(invalidIndex, count).Select(x => (LogLineIndex)x).ToArray();
			logFile.GetColumn(indices, LogFileColumns.ElapsedTime, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(TimeSpan.FromDays(1), "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().BeNull("because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(TimeSpan.FromDays(1), "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		[Test]
		[Ignore("Still not implemented")]
		public void TestGetElapsedTimesBySection()
		{
			var content = new LogEntryBuffer(5, LogFileColumns.Timestamp);
			content.CopyFrom(LogFileColumns.Timestamp, new DateTime?[]
			{
				new DateTime(2017, 12, 19, 15, 49, 0),
				new DateTime(2017, 12, 19, 15, 49, 2),
				new DateTime(2017, 12, 19, 15, 49, 4),
				new DateTime(2017, 12, 19, 15, 49, 8),
				new DateTime(2017, 12, 19, 15, 49, 16)
			});
			var logFile = CreateFromContent(content);
			var values = logFile.GetColumn(new LogFileSection(0, 5), LogFileColumns.ElapsedTime);
			values.Should().Equal(new object[]
			{
				null,
				TimeSpan.FromSeconds(2),
				TimeSpan.FromSeconds(2),
				TimeSpan.FromSeconds(4),
				TimeSpan.FromSeconds(8)
			});
		}

		#endregion

		#region Delta Time

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetDeltaTimesEmptyBySection([Range(from: 0, to: 2)] int count,
		                                            [Range(from: 0, to: 2)] int offset,
		                                            [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new TimeSpan?[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = TimeSpan.FromDays(1);
			}

			logFile.GetColumn(new LogFileSection(0, count), LogFileColumns.DeltaTime, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(TimeSpan.FromDays(1), "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().BeNull("because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(TimeSpan.FromDays(1), "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetDeltaTimesEmptyByIndices([Range(from: -1, to: 1)] int invalidIndex,
		                                            [Range(from: 0, to: 2)] int count,
		                                            [Range(from: 0, to: 2)] int offset,
		                                            [Range(from: 0, to: 2)] int surplus)
		{
			var logFile = CreateEmpty();

			var buffer = new TimeSpan?[offset + count + surplus];
			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i] = TimeSpan.FromDays(1);
			}

			var indices = Enumerable.Range(invalidIndex, count).Select(x => (LogLineIndex) x).ToArray();
			logFile.GetColumn(indices, LogFileColumns.DeltaTime, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(TimeSpan.FromDays(1), "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().BeNull("because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
			}
			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(TimeSpan.FromDays(1), "because we've specified a count and thus values after shouldn't have been touched");
			}
		}

		#endregion

		#region Timestamp

		[Test]
		[Description("Verifies that accessing not-available rows returns default values for that particular column")]
		public void TestGetTimestampEmptyBySection([Values(-1, 0, 1)] int invalidStartIndex,
		                                           [Range(from: 0, to: 2)] int count,
		                                           [Range(from: 0, to: 2)] int offset,
		                                           [Range(from: 0, to: 2)] int surplus)
		{
			using (var logFile = CreateEmpty())
			{
				var buffer = new DateTime?[offset + count + surplus];
				for (int i = 0; i < offset + count + surplus; ++i)
				{
					buffer[i] = new DateTime(2017, 12, 18, 10, 53, 0);
				}

				logFile.GetColumn(new LogFileSection(invalidStartIndex, count),
				                  LogFileColumns.Timestamp,
				                  buffer,
				                  offset);

				for (int i = 0; i < offset; ++i)
				{
					buffer[i].Should().Be(new DateTime(2017, 12, 18, 10, 53, 0), "because we've specified an offset and thus values before that offset shouldn't have been touched");
				}
				for (int i = 0; i < count; ++i)
				{
					buffer[offset + i].Should().BeNull("because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
				}
				for (int i = offset + count; i < offset + count + surplus; ++i)
				{
					buffer[i].Should().Be(new DateTime(2017, 12, 18, 10, 53, 0), "because we've specified a count and thus values after shouldn't have been touched");
				}
			}
		}

		[Test]
		public void TestGetTimestampEmptyByIndices([Range(from: -1, to: 1)] int invalidIndex,
		                                           [Range(from: 0, to: 2)] int count,
		                                           [Range(from: 0, to: 2)] int offset,
		                                           [Range(from: 0, to: 2)] int surplus)
		{
			using (var logFile = CreateEmpty())
			{
				var buffer = new DateTime?[offset + count + surplus];
				for (int i = 0; i < offset + count + surplus; ++i)
				{
					buffer[i] = new DateTime(2017, 12, 18, 10, 53, 0);
				}

				var indices = Enumerable.Range(invalidIndex, count).Select(x => (LogLineIndex) x).ToArray();
				logFile.GetColumn(indices,
				                  LogFileColumns.Timestamp,
				                  buffer,
				                  offset);

				for (int i = 0; i < offset; ++i)
				{
					buffer[i].Should().Be(new DateTime(2017, 12, 18, 10, 53, 0), "because we've specified an offset and thus values before that offset shouldn't have been touched");
				}
				for (int i = 0; i < count; ++i)
				{
					buffer[offset + i].Should().BeNull("because we've accessed a region which is out of range and therefore the default value should've been copied to the buffer");
				}
				for (int i = offset + count; i < offset + count + surplus; ++i)
				{
					buffer[i].Should().Be(new DateTime(2017, 12, 18, 10, 53, 0), "because we've specified a count and thus values after shouldn't have been touched");
				}
			}
		}

		#endregion

		#endregion
	}
}
