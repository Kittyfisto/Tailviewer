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

		#region Well Known Columns

		#region Elapsed Time

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetElapsedTimesEmptyBySection([Range(from: 0, to: 5)] int count,
		                                              [Range(from: 0, to: 3)] int offset)
		{
			var logFile = CreateEmpty();

			var buffer = new TimeSpan?[offset + count];
			for (int i = 0; i < offset + count; ++i)
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
		}

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetElapsedTimesEmptyByIndices([Values(-42, -1, 0, 1, 42)] int invalidIndex,
		                                              [Range(0, 3)] int count,
		                                              [Range(0, 3)] int offset)
		{
			var logFile = CreateEmpty();

			var buffer = new TimeSpan?[offset + count];
			for (int i = 0; i < offset + count; ++i)
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
		public void TestGetDeltaTimesEmptyBySection([Range(0, 5)] int count,
		                                            [Range(0, 3)] int offset)
		{
			var logFile = CreateEmpty();

			var buffer = new TimeSpan?[offset + count];
			for (int i = 0; i < offset + count; ++i)
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
		}

		[Test]
		[Description("Verifies that retrieving a region that is out of range from an empty file simply zeroes out values")]
		public void TestGetDeltaTimesEmptyByIndices([Values(-42, -1, 0, 1, 42)] int invalidIndex,
		                                            [Range(0, 3)] int count,
		                                            [Range(0, 3)] int offset)
		{
			var logFile = CreateEmpty();

			var buffer = new TimeSpan?[offset + count];
			for (int i = 0; i < offset + count; ++i)
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
		}

		#endregion

		#region Timestamp

		[Test]
		[Description("Verifies that accessing not-available rows returns default values for that particular column")]
		public void TestGetTimestampEmptyBySection([Values(-1, 0, 1)] int invalidStartIndex,
		                                           [Range(0, 3)] int count,
		                                           [Range(0, 3)] int offset)
		{
			using (var logFile = CreateEmpty())
			{
				var buffer = new DateTime?[offset + count];
				for (int i = 0; i < offset + count; ++i)
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
			}
		}

		[Test]
		public void TestGetTimestampEmptyByIndices([Values(-42, -1, 0, 1, 42)] int invalidIndex,
		                                           [Range(0, 3)] int count,
		                                           [Range(0, 3)] int offset)
		{
			using (var logFile = CreateEmpty())
			{
				var buffer = new DateTime?[offset + count];
				for (int i = 0; i < offset + count; ++i)
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
			}
		}

		#endregion

		#endregion
	}
}