using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Entries;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	/// <summary>
	/// This class is responsible for testing all <see cref="ILogSource"/> implementations.
	/// </summary>
	public abstract class AbstractLogFileTest
	{
		protected abstract ILogSource CreateEmpty();

		/// <summary>
		///     Creates a new log file with the given content.
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		protected abstract ILogSource CreateFromContent(IReadOnlyLogBuffer content);

		[Test]
		public void TestDebuggerVisualization1()
		{
			var content = new LogBufferArray(2, LogColumns.Minimum);
			content[0].Timestamp = new DateTime(2017, 12, 20, 13, 22, 0);
			content[1].Timestamp = new DateTime(2017, 12, 20, 13, 23, 0);
			var logFile = CreateFromContent(content);
			var visualizer = new LogSourceDebuggerVisualization(logFile);
			var logEntries = visualizer.LogEntries;
			logEntries.Should().NotBeNull();
			logEntries.Should().HaveCount(2);
		}

		[Test]
		public void TestStartEndTimestampEmptyLogFile()
		{
			var logFile = CreateEmpty();
			logFile.GetProperty(GeneralProperties.StartTimestamp).Should().BeNull();
			logFile.GetProperty(GeneralProperties.EndTimestamp).Should().BeNull();
		}

		[Test]
		public void TestStartEndTimestamp1()
		{
			var content = new LogBufferList(LogColumns.Timestamp);
			content.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 21, 14, 11, 0)});
			var logFile = CreateFromContent(content);
			logFile.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2017, 12, 21, 14, 11, 0));
			logFile.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2017, 12, 21, 14, 11, 0));
		}

		[Test]
		public void TestStartEndTimestamp2()
		{
			var content = new LogBufferList(LogColumns.Timestamp);
			content.Add(ReadOnlyLogEntry.Empty);
			content.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 21, 14, 12, 0)});
			content.Add(ReadOnlyLogEntry.Empty);
			content.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 21, 14, 13, 0)});
			content.Add(ReadOnlyLogEntry.Empty);
			var logFile = CreateFromContent(content);
			logFile.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2017, 12, 21, 14, 12, 0));
			logFile.GetProperty(GeneralProperties.EndTimestamp).Should().Be(new DateTime(2017, 12, 21, 14, 13, 0));
		}

		#region Well Known Columns

		[Test]
		public void TestGetNullColumn()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogFileSection(0, 0), null, new int[0], 0)).Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestGetColumnNullIndices()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(null, LogColumns.Index, new LogLineIndex[0], 0)).Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestGetColumnNullBuffer1()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogFileSection(), LogColumns.Index, null, 0)).Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestGetColumnNullBuffer2()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogLineIndex[0], LogColumns.Index, null, 0)).Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestGetColumnBufferTooSmall1()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogFileSection(1, 10),
			                                   LogColumns.OriginalLineNumber,
			                                   new int[9])).Should().Throw<ArgumentException>("because the given buffer is less than the amount of retrieved entries");
		}

		[Test]
		public void TestGetColumnBufferTooSmall2()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogLineIndex[10],
			                                   LogColumns.OriginalLineNumber,
			                                   new int[9])).Should().Throw<ArgumentException>("because the given buffer is less than the amount of retrieved entries");
		}

		[Test]
		public void TestGetColumnDestinationIndexTooSmall1()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogFileSection(1, 10),
			                                   LogColumns.OriginalLineNumber,
			                                   new int[10],
			                                   -1)).Should().Throw<ArgumentOutOfRangeException>("because the given destination index shouldn't be negative");
		}

		[Test]
		public void TestGetColumnDestinationIndexTooSmall2()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogLineIndex[10],
			                                   LogColumns.OriginalLineNumber,
			                                   new int[10],
			                                   -1)).Should().Throw<ArgumentOutOfRangeException>("because the given destination index shouldn't be negative");
		}

		[Test]
		public void TestGetColumnDestinationIndexTooBig1()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogFileSection(1, 10),
			                                   LogColumns.OriginalLineNumber,
			                                   new int[15],
			                                   6)).Should().Throw<ArgumentException>("because the given length and offset are greater than the buffer length");
		}

		[Test]
		public void TestGetColumnDestinationIndexTooBig2()
		{
			var logFile = CreateEmpty();
			new Action(() => logFile.GetColumn(new LogLineIndex[10],
			                                   LogColumns.OriginalLineNumber,
			                                   new int[15],
			                                   6)).Should().Throw<ArgumentException>("because the given length and offset are greater than the buffer length");
		}

		#region Index

		[Test]
		[Description("Verifies that values may be retrieved even when some requested entries are not available")]
		public void TestGetIndexPartiallyInvalidBySection()
		{
			// Index is a computed column (based on the index of the data itself), therefore even if we specify it here, the log file implementation
			// will calculate it based on its own index. We specify it here so that the test assertion below is more understandable.
			// If you (for some reason) need to modify the contents, simply let the index start at 0 and have the line number be index + 1
			// and you're golden.
			//
			// Due to testing lots of ILogFile implementations, we have to offer more rows and prepare its values so they definitely show up
			// as individual log entries in the source (mostly because of MultiLineLogFile as well as TextLogFile).
			var content = new LogBufferList(LogColumns.Index, LogColumns.Timestamp)
			{
				new LogEntry {Index = 0, Timestamp = new DateTime(2021, 02, 13, 13, 20, 41)},
				new LogEntry {Index = 1, Timestamp = new DateTime(2021, 02, 13, 13, 20, 59)},
				new LogEntry {Index = 2, Timestamp = new DateTime(2021, 02, 13, 13, 21, 08)}
			};

			using (var logFile = CreateFromContent(content))
			{
				const int offset = 2;
				const int count = 3;
				const int surplus = 4;

				var buffer = new LogLineIndex[offset + count + surplus];
				for (int i = 0; i < offset + count + surplus; ++i)
				{
					buffer[i] = i + 9001;
				}

				logFile.GetColumn(new LogFileSection(1, 3), // We'll access rows 1 through 3 which means the last access is invalid
				                  LogColumns.Index,
				                  buffer,
				                  offset);

				for (int i = 0; i < offset; ++i)
				{
					buffer[i].Should().Be(i + 9001, "because we've specified an offset and thus values before that offset shouldn't have been touched");
				}

				buffer[offset + 0].Should().Be(content[1].Index, "because we wanted to copy the entry at index 1 of the source");
				buffer[offset + 1].Should().Be(content[2].Index, "because we wanted to copy the entry at index 2 of the source");
				buffer[offset + 2].Should().Be(LogColumns.Index.DefaultValue,
				                               "because even though we wanted to copy the entry at index 3 of the source, that row doesn't exist and should have been filled with the default value for that");

				for (int i = offset + count; i < offset + count + surplus; ++i)
				{
					buffer[i].Should().Be(i + 9001, "because we've specified a count and thus values after shouldn't have been touched");
				}
			}
		}

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

			logFile.GetColumn(new LogFileSection(0, count), LogColumns.Index, buffer, offset);

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
			logFile.GetColumn(indices, LogColumns.Index, buffer, offset);

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
		[Description("Verifies that values may be retrieved even when some requested entries are not available")]
		public void TestGetOriginalIndexPartiallyInvalidBySection()
		{
			// OriginalIndex is a computed column (based on the index of the data itself), therefore even if we specify it here, the log file implementation
			// will calculate it based on its own index. We specify it here so that the test assertion below is more understandable.
			// If you (for some reason) need to modify the contents, simply let the index start at 0 and have the line number be index + 1
			// and you're golden.
			//
			// Due to testing lots of ILogFile implementations, we have to offer more rows and prepare its values so they definitely show up
			// as individual log entries in the source (mostly because of MultiLineLogFile as well as TextLogFile).
			var content = new LogBufferList(LogColumns.OriginalIndex, LogColumns.Timestamp)
			{
				new LogEntry {OriginalIndex = 0, Timestamp = new DateTime(2021, 02, 13, 13, 20, 41)},
				new LogEntry {OriginalIndex = 1, Timestamp = new DateTime(2021, 02, 13, 13, 20, 59)},
				new LogEntry {OriginalIndex = 2, Timestamp = new DateTime(2021, 02, 13, 13, 21, 08)}
			};

			using (var logFile = CreateFromContent(content))
			{
				const int offset = 2;
				const int count = 3;
				const int surplus = 4;

				var buffer = new LogLineIndex[offset + count + surplus];
				for (int i = 0; i < offset + count + surplus; ++i)
				{
					buffer[i] = i + 9001;
				}

				logFile.GetColumn(new LogFileSection(1, 3), // We'll access rows 1 through 3 which means the last access is invalid
				                  LogColumns.OriginalIndex,
				                  buffer,
				                  offset);

				for (int i = 0; i < offset; ++i)
				{
					buffer[i].Should().Be(i + 9001, "because we've specified an offset and thus values before that offset shouldn't have been touched");
				}

				buffer[offset + 0].Should().Be(content[1].OriginalIndex, "because we wanted to copy the entry at index 1 of the source");
				buffer[offset + 1].Should().Be(content[2].OriginalIndex, "because we wanted to copy the entry at index 2 of the source");
				buffer[offset + 2].Should().Be(LogColumns.OriginalIndex.DefaultValue,
				                               "because even though we wanted to copy the entry at index 3 of the source, that row doesn't exist and should have been filled with the default value for that");

				for (int i = offset + count; i < offset + count + surplus; ++i)
				{
					buffer[i].Should().Be(i + 9001, "because we've specified a count and thus values after shouldn't have been touched");
				}
			}
		}

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

			logFile.GetColumn(new LogFileSection(0, count), LogColumns.OriginalIndex, buffer, offset);

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
			logFile.GetColumn(indices, LogColumns.OriginalIndex, buffer, offset);

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
			var content = new LogBufferList(LogColumns.Timestamp);
			content.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 21, 0, 0, 0)});
			content.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 21, 0, 0, 1)});
			content.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 21, 0, 0, 2)});
			content.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 21, 0, 0, 3)});
			var logFile = CreateFromContent(content);

			var indices = new LogLineIndex[5];
			indices[0] = new LogLineIndex(42);
			indices[4] = new LogLineIndex(9001);

			logFile.GetColumn(new LogFileSection(1, 3), LogColumns.OriginalIndex, indices, 1);
			indices[0].Should().Be(42, "because the original value shouldn't have been written over");
			indices[1].Should().Be(1);
			indices[2].Should().Be(2);
			indices[3].Should().Be(3);
			indices[4].Should().Be(9001, "because the original value shouldn't have been written over");
		}

		[Test]
		public void TestGetOriginalIndicesByIndices()
		{
			var content = new LogBufferList(LogColumns.Timestamp);
			content.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 21, 0, 0, 0)});
			content.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 21, 0, 0, 1)});
			content.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 21, 0, 0, 2)});
			content.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 21, 0, 0, 3)});
			var logFile = CreateFromContent(content);

			var indices = new LogLineIndex[5];
			indices[0] = new LogLineIndex(42);
			indices[4] = new LogLineIndex(9001);

			logFile.GetColumn(new LogLineIndex[] {3,1,2}, LogColumns.OriginalIndex, indices, 1);
			indices[0].Should().Be(42, "because the original value shouldn't have been written over");
			indices[1].Should().Be(3);
			indices[2].Should().Be(1);
			indices[3].Should().Be(2);
			indices[4].Should().Be(9001, "because the original value shouldn't have been written over");
		}

		#endregion

		#region Line Number

		[Test]
		[Description("Verifies that values may be retrieved even when some requested entries are not available")]
		public void TestGetLineNumberPartiallyInvalidBySection()
		{
			// LineNumber is a computed column (based on the log index), therefore even if we specify it here, the log file implementation
			// will calculate it based on its own index. We specify it here so that the test assertion below is more understandable.
			// If you (for some reason) need to modify the contents, simply let the index start at 0 and have the line number be index + 1
			// and you're golden.
			//
			// Due to testing lots of ILogFile implementations, we have to offer more rows and prepare its values so they definitely show up
			// as individual log entries in the source (mostly because of MultiLineLogFile as well as TextLogFile).
			var content = new LogBufferList(LogColumns.Index, LogColumns.LineNumber, LogColumns.Timestamp)
			{
				new LogEntry {Index = 0, LineNumber = 1, Timestamp = new DateTime(2021, 02, 13, 13, 20, 41)},
				new LogEntry {Index = 1, LineNumber = 2, Timestamp = new DateTime(2021, 02, 13, 13, 20, 59)},
				new LogEntry {Index = 2, LineNumber = 3, Timestamp = new DateTime(2021, 02, 13, 13, 21, 08)}
			};

			using (var logFile = CreateFromContent(content))
			{
				const int offset = 2;
				const int count = 3;
				const int surplus = 4;

				var buffer = new int[offset + count + surplus];
				for (int i = 0; i < offset + count + surplus; ++i)
				{
					buffer[i] = i + 9001;
				}

				logFile.GetColumn(new LogFileSection(1, 3), // We'll access rows 1 through 3 which means the last access is invalid
				                  LogColumns.LineNumber,
				                  buffer,
				                  offset);

				for (int i = 0; i < offset; ++i)
				{
					buffer[i].Should().Be(i + 9001, "because we've specified an offset and thus values before that offset shouldn't have been touched");
				}

				buffer[offset + 0].Should().Be(content[1].LineNumber, "because we wanted to copy the entry at index 1 of the source");
				buffer[offset + 1].Should().Be(content[2].LineNumber, "because we wanted to copy the entry at index 2 of the source");
				buffer[offset + 2].Should().Be(LogColumns.LineNumber.DefaultValue,
				                               "because even though we wanted to copy the entry at index 3 of the source, that row doesn't exist and should have been filled with the default value for that");

				for (int i = offset + count; i < offset + count + surplus; ++i)
				{
					buffer[i].Should().Be(i + 9001, "because we've specified a count and thus values after shouldn't have been touched");
				}
			}
		}

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

			logFile.GetColumn(new LogFileSection(0, count), LogColumns.LineNumber, buffer, offset);

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
			logFile.GetColumn(indices, LogColumns.LineNumber, buffer, offset);

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
		[Description("Verifies that values may be retrieved even when some requested entries are not available")]
		public void TestGetOriginalLineNumberPartiallyInvalidBySection()
		{
			// OriginalLineNumber is a computed column (based on the original log index), therefore even if we specify it here, the log file implementation
			// will calculate it based on its own index. We specify it here so that the test assertion below is more understandable.
			// If you (for some reason) need to modify the contents, simply let the index start at 0 and have the line number be index + 1
			// and you're golden.
			//
			// Due to testing lots of ILogFile implementations, we have to offer more rows and prepare its values so they definitely show up
			// as individual log entries in the source (mostly because of MultiLineLogFile as well as TextLogFile).
			var content = new LogBufferList(LogColumns.Index, LogColumns.OriginalLineNumber, LogColumns.Timestamp)
			{
				new LogEntry {Index = 0, OriginalLineNumber = 1, Timestamp = new DateTime(2021, 02, 13, 13, 20, 41)},
				new LogEntry {Index = 1, OriginalLineNumber = 2, Timestamp = new DateTime(2021, 02, 13, 13, 20, 59)},
				new LogEntry {Index = 2, OriginalLineNumber = 3, Timestamp = new DateTime(2021, 02, 13, 13, 21, 08)}
			};

			using (var logFile = CreateFromContent(content))
			{
				const int offset = 2;
				const int count = 3;
				const int surplus = 4;

				var buffer = new int[offset + count + surplus];
				for (int i = 0; i < offset + count + surplus; ++i)
				{
					buffer[i] = i + 9001;
				}

				logFile.GetColumn(new LogFileSection(1, 3), // We'll access rows 1 through 3 which means the last access is invalid
				                  LogColumns.OriginalLineNumber,
				                  buffer,
				                  offset);

				for (int i = 0; i < offset; ++i)
				{
					buffer[i].Should().Be(i + 9001, "because we've specified an offset and thus values before that offset shouldn't have been touched");
				}

				buffer[offset + 0].Should().Be(content[1].OriginalLineNumber, "because we wanted to copy the entry at index 1 of the source");
				buffer[offset + 1].Should().Be(content[2].OriginalLineNumber, "because we wanted to copy the entry at index 2 of the source");
				buffer[offset + 2].Should().Be(LogColumns.OriginalLineNumber.DefaultValue,
				                               "because even though we wanted to copy the entry at index 3 of the source, that row doesn't exist and should have been filled with the default value for that");

				for (int i = offset + count; i < offset + count + surplus; ++i)
				{
					buffer[i].Should().Be(i + 9001, "because we've specified a count and thus values after shouldn't have been touched");
				}
			}
		}

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

			logFile.GetColumn(new LogFileSection(0, count), LogColumns.OriginalLineNumber, buffer, offset);

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
			logFile.GetColumn(indices, LogColumns.OriginalLineNumber, buffer, offset);

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
		[Description("Verifies that values may be retrieved even when some requested entries are not available")]
		public void TestGetLogLevelPartiallyInvalidBySection()
		{
			// Due to testing lots of ILogFile implementations, we have to offer more rows and prepare its values so they definitely show up
			// as individual log entries in the source (mostly because of MultiLineLogFile as well as TextLogFile).
			var content = new LogBufferList(LogColumns.Index, LogColumns.Timestamp, LogColumns.RawContent, LogColumns.LogLevel)
			{
				new LogEntry {Index = 0, Timestamp = new DateTime(2021, 02, 13, 13, 20, 41), RawContent = "INFO: A", LogLevel = LevelFlags.Info},
				new LogEntry {Index = 1, Timestamp = new DateTime(2021, 02, 13, 13, 20, 59), RawContent = "WARNING: B", LogLevel = LevelFlags.Warning},
				new LogEntry {Index = 2, Timestamp = new DateTime(2021, 02, 13, 13, 21, 08), RawContent = "DEBUG: C", LogLevel = LevelFlags.Debug}
			};

			using (var logFile = CreateFromContent(content))
			{
				const int offset = 2;
				const int count = 3;
				const int surplus = 4;

				var buffer = new LevelFlags[offset + count + surplus];
				for (int i = 0; i < offset + count + surplus; ++i)
				{
					buffer[i] = LevelFlags.Trace;
				}

				logFile.GetColumn(new LogFileSection(1, 3), // We'll access rows 1 through 3 which means the last access is invalid
				                  LogColumns.LogLevel,
				                  buffer,
				                  offset);

				for (int i = 0; i < offset; ++i)
				{
					buffer[i] = LevelFlags.Trace;
				}

				buffer[offset + 0].Should().Be(content[1].LogLevel, "because we wanted to copy the entry at index 1 of the source");
				buffer[offset + 1].Should().Be(content[2].LogLevel, "because we wanted to copy the entry at index 2 of the source");
				buffer[offset + 2].Should().Be(LogColumns.LogLevel.DefaultValue,
				                               "because even though we wanted to copy the entry at index 3 of the source, that row doesn't exist and should have been filled with the default value for that");

				for (int i = offset + count; i < offset + count + surplus; ++i)
				{
					buffer[i] = LevelFlags.Trace;
				}
			}
		}

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

			logFile.GetColumn(new LogFileSection(0, count), LogColumns.LogLevel, buffer, offset);

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
			logFile.GetColumn(indices, LogColumns.LogLevel, buffer, offset);

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
		[Description("Verifies that values may be retrieved even when some requested entries are not available")]
		public void TestGetElapsedTimesPartiallyInvalidBySection()
		{
			// The ElapsedTime column values are computed by subtracting the timestamps of neighboring log entries,
			// therefore in order to create a log file which offers the ElapsedTime column to us, we have to fill it
			// with log entries containing a timestamp.
			var content = new LogBufferList(LogColumns.Index, LogColumns.Timestamp)
			{
				new LogEntry {Index = 0, Timestamp = new DateTime(2021, 02, 13, 13, 47, 41)},
				new LogEntry {Index = 1, Timestamp = new DateTime(2021, 02, 13, 13, 47, 59)},
				new LogEntry {Index = 2, Timestamp = new DateTime(2021, 02, 13, 13, 49, 08)}
			};

			using (var logFile = CreateFromContent(content))
			{
				const int offset = 5;
				const int count = 3;
				const int surplus = 4;

				var buffer = new TimeSpan?[offset + count + surplus];
				for (int i = 0; i < offset + count + surplus; ++i)
				{
					buffer[i] = TimeSpan.FromDays(1);
				}

				logFile.GetColumn(new LogFileSection(1, count), LogColumns.ElapsedTime, buffer, offset);

				for (int i = 0; i < offset; ++i)
				{
					buffer[i].Should().Be(TimeSpan.FromDays(1), "because we've specified an offset and thus values before that offset shouldn't have been touched");
				}

				buffer[offset + 0].Should().Be(TimeSpan.FromSeconds(18), "because we wanted to copy the elapsed difference between the second and first log entry");
				buffer[offset + 1].Should().Be(TimeSpan.FromSeconds(87), "because we wanted to copy the time difference between the third and first entry of the source");
				buffer[offset + 2].Should().Be(LogColumns.DeltaTime.DefaultValue,
				                               "because source doesn't have a fourth entry and the buffer should have been filled with the default value for that");

				for (int i = offset + count; i < offset + count + surplus; ++i)
				{
					buffer[i].Should().Be(TimeSpan.FromDays(1), "because we've specified a count and thus values after shouldn't have been touched");
				}
			}
		}

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

			logFile.GetColumn(new LogFileSection(0, count), LogColumns.ElapsedTime, buffer, offset);

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
			logFile.GetColumn(indices, LogColumns.ElapsedTime, buffer, offset);

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
		public void TestGetElapsedTimesBySection()
		{
			var content = new LogBufferArray(5, LogColumns.Timestamp);
			content.CopyFrom(LogColumns.Timestamp, new DateTime?[]
			{
				new DateTime(2017, 12, 19, 15, 49, 1),
				new DateTime(2017, 12, 19, 15, 49, 3),
				new DateTime(2017, 12, 19, 15, 49, 5),
				new DateTime(2017, 12, 19, 15, 49, 9),
				new DateTime(2017, 12, 19, 15, 49, 17)
			});
			var logFile = CreateFromContent(content);
			var values = logFile.GetColumn(new LogFileSection(0, 5), LogColumns.ElapsedTime);
			values.Should().Equal(new object[]
			{
				TimeSpan.FromSeconds(0),
				TimeSpan.FromSeconds(2),
				TimeSpan.FromSeconds(4),
				TimeSpan.FromSeconds(8),
				TimeSpan.FromSeconds(16)
			});
		}

		#endregion

		#region Delta Time
		
		[Test]
		[Description("Verifies that values may be retrieved even when some requested entries are not available")]
		public void TestGetDeltaTimesPartiallyInvalidBySection()
		{
			// The DeltaTime column values are computed by subtracting the timestamps of neighboring log entries,
			// therefore in order to create a log file which offers the DeltaTime column to us, we have to fill it
			// with log entries containing a timestamp.
			var content = new LogBufferList(LogColumns.Index, LogColumns.Timestamp)
			{
				new LogEntry {Index = 0, Timestamp = new DateTime(2021, 02, 13, 13, 41, 41)},
				new LogEntry {Index = 1, Timestamp = new DateTime(2021, 02, 13, 13, 41, 59)},
				new LogEntry {Index = 2, Timestamp = new DateTime(2021, 02, 13, 13, 42, 08)}
			};

			using (var logFile = CreateFromContent(content))
			{
				const int offset = 4;
				const int count = 3;
				const int surplus = 2;

				var buffer = new TimeSpan?[offset + count + surplus];
				for (int i = 0; i < offset + count + surplus; ++i)
				{
					buffer[i] = TimeSpan.FromDays(1);
				}

				logFile.GetColumn(new LogFileSection(1, count), LogColumns.DeltaTime, buffer, offset);

				for (int i = 0; i < offset; ++i)
				{
					buffer[i].Should().Be(TimeSpan.FromDays(1), "because we've specified an offset and thus values before that offset shouldn't have been touched");
				}

				buffer[offset + 0].Should().Be(TimeSpan.FromSeconds(18), "because we wanted to copy the time difference between the first and second entry of the source");
				buffer[offset + 1].Should().Be(TimeSpan.FromSeconds(9), "because we wanted to copy the time difference between the third and second entry of the source");
				buffer[offset + 2].Should().Be(LogColumns.DeltaTime.DefaultValue,
				                               "because source doesn't have a fourth entry and the buffer should have been filled with the default value for that");

				for (int i = offset + count; i < offset + count + surplus; ++i)
				{
					buffer[i].Should().Be(TimeSpan.FromDays(1), "because we've specified a count and thus values after shouldn't have been touched");
				}
			}
		}

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

			logFile.GetColumn(new LogFileSection(0, count), LogColumns.DeltaTime, buffer, offset);

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
			logFile.GetColumn(indices, LogColumns.DeltaTime, buffer, offset);

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
		[Description("Verifies that values may be retrieved even when some requested entries are not available")]
		public void TestGetTimestampPartiallyInvalidBySection()
		{
			var content = new LogBufferList(LogColumns.Index, LogColumns.RawContent, LogColumns.Timestamp)
			{
				new LogEntry {Index = 0, Timestamp = new DateTime(2021, 02, 13, 13, 20, 41)},
				new LogEntry {Index = 1, Timestamp = new DateTime(2021, 02, 13, 13, 20, 59)},
				new LogEntry {Index = 2, Timestamp = new DateTime(2021, 02, 13, 13, 21, 08)}
			};

			using (var logFile = CreateFromContent(content))
			{
				const int offset = 2;
				const int count = 3;
				const int surplus = 4;

				var buffer = new DateTime?[offset + count + surplus];
				for (int i = 0; i < offset + count + surplus; ++i)
				{
					buffer[i] = new DateTime(2017, 12, 18, 10, 53, 0);
				}

				logFile.GetColumn(new LogFileSection(1, 3), // We'll access rows 1 through 3 which means the last access is invalid
				                  LogColumns.Timestamp,
				                  buffer,
				                  offset);

				for (int i = 0; i < offset; ++i)
				{
					buffer[i].Should().Be(new DateTime(2017, 12, 18, 10, 53, 0), "because we've specified an offset and thus values before that offset shouldn't have been touched");
				}

				buffer[offset + 0].Should().Be(content[1].Timestamp, "because we wanted to copy the entry at index 1 of the source");
				buffer[offset + 1].Should().Be(content[2].Timestamp, "because we wanted to copy the entry at index 2 of the source");
				buffer[offset + 2].Should().Be(LogColumns.Timestamp.DefaultValue,
				                               "because even though we wanted to copy the entry at index 3 of the source, that row doesn't exist and should have been filled with the default value for that");

				for (int i = offset + count; i < offset + count + surplus; ++i)
				{
					buffer[i].Should().Be(new DateTime(2017, 12, 18, 10, 53, 0), "because we've specified a count and thus values after shouldn't have been touched");
				}
			}
		}

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
				                  LogColumns.Timestamp,
				                  buffer,
				                  offset);

				for (int i = 0; i < offset; ++i)
				{
					buffer[i].Should().Be(new DateTime(2017, 12, 18, 10, 53, 0), "because we've specified an offset and thus values before that offset shouldn't have been touched");
				}
				for (int i = 0; i < count; ++i)
				{
					buffer[offset + i].Should().BeNull("because we've accessed a region which is out of range and therefore the default value should have been copied to the buffer");
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
				                  LogColumns.Timestamp,
				                  buffer,
				                  offset);

				for (int i = 0; i < offset; ++i)
				{
					buffer[i].Should().Be(new DateTime(2017, 12, 18, 10, 53, 0), "because we've specified an offset and thus values before that offset shouldn't have been touched");
				}
				for (int i = 0; i < count; ++i)
				{
					buffer[offset + i].Should().BeNull("because we've accessed a region which is out of range and therefore the default value should have been copied to the buffer");
				}
				for (int i = offset + count; i < offset + count + surplus; ++i)
				{
					buffer[i].Should().Be(new DateTime(2017, 12, 18, 10, 53, 0), "because we've specified a count and thus values after shouldn't have been touched");
				}
			}
		}

		#endregion

		[Test]
		public void TestUnknownProperty()
		{
			using (var logFile = CreateEmpty())
			{
				var customDefaultValue = "Shazam!";
				var myTypedProperty = new WellKnownReadOnlyProperty<string>("My current movie", customDefaultValue);

				logFile.GetProperty(myTypedProperty).Should().Be(customDefaultValue,
				                                                "because the log doesn't have that property and should returns default value instead");
				logFile.GetProperty((IReadOnlyPropertyDescriptor)myTypedProperty).Should().Be(customDefaultValue,
				                                                "because the log doesn't have that property and should returns default value instead");
			}
		}

		#endregion

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/282")]
		[Description("Verifies that the log file implementation appears totally empty after Dispose() has been called")]
		public void TestDisposeData()
		{
			var content = new LogBufferList(LogColumns.Index, LogColumns.RawContent, LogColumns.Timestamp)
			{
				new LogEntry {Index = 0, Timestamp = new DateTime(2021, 02, 13, 13, 20, 41)},
				new LogEntry {Index = 1, Timestamp = new DateTime(2021, 02, 13, 13, 20, 59)},
				new LogEntry {Index = 2, Timestamp = new DateTime(2021, 02, 13, 13, 21, 08)}
			};

			using (var logFile = CreateFromContent(content))
			{
				logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(3);

				logFile.Dispose();
				logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(0);
				var entries = logFile.GetEntries(new LogFileSection(0, 3));
				entries[0].Index.Should().Be(LogLineIndex.Invalid, "because the log entry shouldn't be present in memory anymore");
				entries[1].Index.Should().Be(LogLineIndex.Invalid, "because the log entry shouldn't be present in memory anymore");
				entries[2].Index.Should().Be(LogLineIndex.Invalid, "because the log entry shouldn't be present in memory anymore");

				logFile.Properties.Should().BeEmpty();
				var values = logFile.GetAllProperties();
				values.Properties.Should().BeEmpty();

				logFile.GetProperty(GeneralProperties.PercentageProcessed).Should()
				       .Be(GeneralProperties.PercentageProcessed.DefaultValue);
			}
		}
	}
}
