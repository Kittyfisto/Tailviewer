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
			var items = visualizer.Items;
			items.Should().NotBeNull();
			items.Should().HaveCount(2);
		}

		#region Well Known Columns

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
		public void TestGetIndexEmptyByIndices([Values(-42, -1, 0, 1, 42)] int invalidIndex,
		                                       [Range(from: 0, to: 3)] int count,
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
		public void TestGetOriginalIndexEmptyByIndices([Values(-42, -1, 0, 1, 42)] int invalidIndex,
		                                               [Range(from: 0, to: 3)] int count,
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
		public void TestGetLineNumberEmptyByIndices([Values(-42, -1, 0, 1, 42)] int invalidIndex,
		                                            [Range(from: 0, to: 3)] int count,
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
		public void TestGetOriginalLineNumberEmptyByIndices([Values(-42, -1, 0, 1, 42)] int invalidIndex,
		                                                    [Range(from: 0, to: 3)] int count,
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
		public void TestGetLogLevelEmptyByIndices([Values(-42, -1, 0, 1, 42)] int invalidIndex,
		                                          [Range(from: 0, to: 3)] int count,
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
		public void TestGetElapsedTimesEmptyByIndices([Values(-42, -1, 0, 1, 42)] int invalidIndex,
		                                              [Range(from: 0, to: 3)] int count,
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
		public void TestGetDeltaTimesEmptyByIndices([Values(-42, -1, 0, 1, 42)] int invalidIndex,
		                                            [Range(from: 0, to: 3)] int count,
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
		                                           [Range(from: 0, to: 3)] int count,
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
		public void TestGetTimestampEmptyByIndices([Values(-42, -1, 0, 1, 42)] int invalidIndex,
		                                           [Range(from: 0, to: 3)] int count,
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