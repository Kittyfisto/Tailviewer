using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Core.Sources.Text.Streaming;
using Tailviewer.Plugins;
using Tailviewer.Test;
using Tailviewer.Test.BusinessLogic.Sources.Text;

namespace Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text.Streaming
{
	[TestFixture]
	public sealed class StreamingTextLogSourceTest
	{
		private ServiceContainer _serviceContainer;
		private ManualTaskScheduler _taskScheduler;
		private SimpleLogFileFormatMatcher _formatMatcher;
		private SimpleLogEntryParserPlugin _logEntryParserPlugin;

		public static IReadOnlyList<Encoding> Encodings => LineOffsetDetectorTest.Encodings;

		public static IReadOnlyList<string> ReferenceFiles => new[]
		{
			@"TestData\2MB.txt",
			@"TestData\20MB.txt",
		};

		[SetUp]
		public void SetUp()
		{
			_serviceContainer = new ServiceContainer();
			_taskScheduler = new ManualTaskScheduler();
			_formatMatcher = new SimpleLogFileFormatMatcher(LogFileFormats.GenericText);
			_logEntryParserPlugin = new SimpleLogEntryParserPlugin();

			_serviceContainer.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_serviceContainer.RegisterInstance<ILogFileFormatMatcher>(_formatMatcher);
			_serviceContainer.RegisterInstance<ILogEntryParserPlugin>(_logEntryParserPlugin);

		}

		private string GetUniqueNonExistingFileName()
		{
			var fileName = PathEx.GetTempFileName();
			if (File.Exists(fileName))
				File.Delete(fileName);

			TestContext.WriteLine("FileName: {0}", fileName);
			return fileName;
		}

		private StreamingTextLogSource Create(string fileName)
		{
			return Create(fileName, Encoding.Default);
		}

		private StreamingTextLogSource Create(string fileName, Encoding encoding)
		{
			return new StreamingTextLogSource(_taskScheduler, fileName, LogFileFormats.GenericText, encoding);
		}

		private IReadOnlyLogEntry GetEntry(StreamingTextLogSource logSource, LogLineIndex index)
		{
			var readTask = Task.Factory.StartNew(() => logSource.GetEntry(index));
			logSource.Property(x => x.HasPendingReadRequests).ShouldEventually().BeTrue();
			_taskScheduler.RunOnce();
			readTask.Wait();
			//readTask.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue("because the task should have been finished now that we've let the scheduler run");
			return readTask.Result;
		}

		private IReadOnlyLogBuffer GetEntries(StreamingTextLogSource logSource)
		{
			return GetEntries(logSource, new LogSourceSection(0, logSource.GetProperty(GeneralProperties.LogEntryCount)));
		}

		private IReadOnlyLogBuffer GetEntries(StreamingTextLogSource logSource, IReadOnlyList<LogLineIndex> sourceSection)
		{
			var readTask = Task.Factory.StartNew(() => logSource.GetEntries(sourceSection));
			logSource.Property(x => x.HasPendingReadRequests).ShouldEventually().BeTrue();
			_taskScheduler.RunOnce();
			readTask.Wait();
			//readTask.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue("because the task should have been finished now that we've let the scheduler run");
			return readTask.Result;
		}

		private IReadOnlyList<LogSourceModification> AddListener(StreamingTextLogSource logSource, int maxCount)
		{
			var listener = new Mock<ILogSourceListener>();
			var modifications = new List<LogSourceModification>();
			listener.Setup(x => x.OnLogFileModified(logSource, It.IsAny<LogSourceModification>()))
			        .Callback((ILogSource _, LogSourceModification modification) =>
			        {
				        modifications.Add(modification);
			        });
			logSource.AddListener(listener.Object, TimeSpan.Zero, maxCount);
			return modifications;
		}

		[Test]
		public void TestConstruction()
		{
			var logFile = Create(GetUniqueNonExistingFileName(), Encoding.Default);
			logFile.GetProperty(TextProperties.RequiresBuffer).Should().BeTrue();
			logFile.GetProperty(TextProperties.LineCount).Should().Be(0);
			logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(0);
			logFile.GetProperty(GeneralProperties.Size).Should().BeNull("because the log file didn't even have enough time to check the source");
			logFile.GetProperty(GeneralProperties.Created).Should().BeNull("because the log file didn't even have enough time to check the source");
			logFile.GetProperty(GeneralProperties.LastModified).Should().BeNull("because the log file didn't even have enough time to check the source");
			logFile.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.Zero);
			logFile.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.None, "because the log file didn't have enough time to check the source");
		}

		#region Static data

		[Test]
		public void TestFileDoesNotExist([Values(1, 2, 3)] int numReadOperations)
		{
			var logFile = Create(GetUniqueNonExistingFileName(), Encoding.Default);
			_taskScheduler.Run(numReadOperations);
			
			logFile.GetProperty(TextProperties.LineCount).Should().Be(0);
			logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(0);
			logFile.GetProperty(GeneralProperties.Size).Should().BeNull("because the source file does not exist");
			logFile.GetProperty(GeneralProperties.Created).Should().BeNull("because the source file does not exist");
			logFile.GetProperty(GeneralProperties.LastModified).Should().BeNull("because the source file does not exist");
			logFile.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist, "because the source file does not exist");
			logFile.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because we've checked that the source doesn't exist and thus there's nothing more to process");
		}

		[Test]
		public void TestAccessInvalidRegion()
		{
			var logFile = Create(GetUniqueNonExistingFileName(), Encoding.Default);
			_taskScheduler.RunOnce();

			var entries = GetEntries(logFile, new LogSourceSection(0, 1));
			entries.Count.Should().Be(1);
			entries[0].Index.Should().Be(LogLineIndex.Invalid);
			entries[0].RawContent.Should().Be(GeneralColumns.RawContent.DefaultValue);
		}

		[Test]
		[Description("Verifies that reading an empty file once yields correct ")]
		public void TestEmptyFile([Values(1, 2, 3)] int numReadOperations)
		{
			var fileName = @"TestData\Empty.txt";
			var info = FileFingerprint.FromFile(fileName);

			var logFile = Create(fileName, Encoding.Default);
			_taskScheduler.Run(numReadOperations);
			logFile.GetProperty(TextProperties.LineCount).Should().Be(0, "because the file is empty");
			logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(0, "because the file is empty");
			logFile.GetProperty(GeneralProperties.Size).Should().Be(Size.Zero, "because the file is empty");
			logFile.GetProperty(GeneralProperties.Created).Should().Be(info.Created);
			logFile.GetProperty(GeneralProperties.LastModified).Should().Be(info.LastModified);
			logFile.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.None, "because the source file does exist and can be accessed");
			logFile.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because we've checked that the source doesn't exist and thus there's nothing more to process");

			var indices = logFile.GetColumn(new LogSourceSection(0, 1), StreamingTextLogSource.LineOffsetInBytes);
			indices[0].Should().Be(StreamingTextLogSource.LineOffsetInBytes.DefaultValue);

			var entries = GetEntries(logFile);
			entries.Should().BeEmpty();
		}

		[Test]
		public void Test1Line([Values(1, 2, 3)] int numReadOperations)
		{
			var fileName = @"TestData\1Line.txt";
			var info = FileFingerprint.FromFile(fileName);

			var logFile = Create(fileName, Encoding.Default);
			_taskScheduler.Run(numReadOperations);
			
			logFile.GetProperty(TextProperties.LineCount).Should().Be(1);
			logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);
			logFile.GetProperty(GeneralProperties.Size).Should().Be(Size.FromBytes(109));
			logFile.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

			var indices = logFile.GetColumn(new LogSourceSection(0, 1), StreamingTextLogSource.LineOffsetInBytes);
			indices[0].Should().Be(0, "because the first line starts at an offset of 0 bytes wrt the start of the file");

			var entries = GetEntries(logFile);
			entries.Should().HaveCount(1);
			entries.Columns.Should().Equal(new IColumnDescriptor[]{GeneralColumns.Index, StreamingTextLogSource.LineOffsetInBytes, GeneralColumns.RawContent});
			entries[0].Index.Should().Be(0);
			entries[0].RawContent.Should().Be(@"[00:00:01] git clone -q --branch=master https://github.com/Kittyfisto/SharpRemote.git C:\projects\sharpremote");
		}

		[Test]
		public void Test2Lines()
		{
			// TODO: Rename to 3 lines because that filename is just plain wrong
			var fileName = @"TestData\2Lines.txt";
			var info = FileFingerprint.FromFile(fileName);

			var logFile = Create(fileName, Encoding.UTF8);
			_taskScheduler.RunOnce();

			logFile.GetProperty(TextProperties.LineCount).Should().Be(3, "because there's 3 lines, the last one is empty though");
			logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(3, "because there's 3 lines, the last one is empty though");
			logFile.GetProperty(GeneralProperties.Size).Should().Be(Size.FromBytes(new FileInfo(fileName).Length)); //< Git fucks with the file length due to replacing line endings => we can't hard code it here
			logFile.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

			var indices = logFile.GetColumn(new LogSourceSection(0, 3), StreamingTextLogSource.LineOffsetInBytes);
			indices[0].Should().Be(3, "because the first line starts right after the preamble (also called byte order mark, BOM), which, for UTF-8, is 3 bytes long");
			indices[1].Should().BeInRange(165L, 166L, "because git fucks with line endings and thus the offset might differ");

			var entries = GetEntries(logFile);
			entries.Should().HaveCount(3);
			entries.Columns.Should().Equal(new IColumnDescriptor[]{GeneralColumns.Index, StreamingTextLogSource.LineOffsetInBytes, GeneralColumns.RawContent});
			entries[0].Index.Should().Be(0);
			entries[0].RawContent.Should().Be("2015-10-07 19:50:58,981 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver");
			entries[1].Index.Should().Be(1);
			entries[1].RawContent.Should().Be("2015-10-07 19:50:58,997 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1");
		}

		[Test]
		public void TestReadOneSentence([ValueSource(nameof(Encodings))] Encoding encoding)
		{
			var line = "that feeling when you bite into a pickle and it's a little squishier than you expected";

			var fileName = GetUniqueNonExistingFileName();
			using (var stream = File.OpenWrite(fileName))
			using (var writer = new StreamWriter(stream, encoding))
			{
				writer.Write(line);
			}

			var logFile = Create(fileName, encoding);
			_taskScheduler.RunOnce();

			logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);
			var indices = logFile.GetColumn(new LogSourceSection(0, 2), StreamingTextLogSource.LineOffsetInBytes);
			var expectedOffset = encoding.GetPreamble().Length;
			indices[0].Should().Be(expectedOffset, "");
			indices[1].Should().Be(-1);

			var entries = GetEntries(logFile);
			entries.Should().HaveCount(1);
			entries[0].Index.Should().Be(0);
			entries[0].RawContent.Should().Be(line);
		}

		[Test]
		public void TestSkipPreambleIfMissing()
		{
			var writtenEncoding = new UTF8Encoding(false);

			var line = "Hello, World!";
			var fileName = GetUniqueNonExistingFileName();
			using (var stream = File.OpenWrite(fileName))
			using (var writer = new StreamWriter(stream, writtenEncoding))
			{
				writer.Write(line);
			}

			var logFile = Create(fileName, new UTF8Encoding(true));
			_taskScheduler.RunOnce();

			var entries = GetEntries(logFile);
			entries.Count.Should().Be(1);
			entries[0].RawContent.Should().Be(line);
		}

		[Test]
		public void TestInvokeListenerEventually()
		{
			var line = "Hello, World!";
			var fileName = GetUniqueNonExistingFileName();
			using (var stream = File.OpenWrite(fileName))
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(line);
			}

			var logFile = Create(fileName, new UTF8Encoding(true));
			var listener = new Mock<ILogSourceListener>();
			var modifications = new List<LogSourceModification>();
			listener.Setup(x => x.OnLogFileModified(logFile, It.IsAny<LogSourceModification>()))
			        .Callback((ILogSource _, LogSourceModification modification) =>
			        {
				        modifications.Add(modification);
			        });
			var maxWaitTime = TimeSpan.FromMilliseconds(500);
			logFile.AddListener(listener.Object, maxWaitTime, 500);
			modifications.Should().Equal(new object[] {LogSourceModification.Reset()});

			_taskScheduler.RunOnce();
			logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1, "because there's one log entry in the file");
			modifications.Should().Equal(new object[] {LogSourceModification.Reset()}, "because not enough time has elapsed and thus the log source may not have notified the listener just yet");


			Thread.Sleep(maxWaitTime);
			_taskScheduler.RunOnce();
			logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1, "because there's still one log entry in the file");
			modifications.Should().Equal(new object[] {LogSourceModification.Reset(), LogSourceModification.Appended(0, 1)}, "because enough time has passed for the log file to notify us at this point in time");
		}

		#region Reference Files

		[Test]
		public void TestRead_AccesAll([ValueSource(nameof(ReferenceFiles))] string fileName)
		{
			var actualLines = File.ReadAllText(fileName).Split(new []{"\n"}, StringSplitOptions.None).Select(x => x.TrimEnd('\r')).ToList();
			var encoding = DetectEncoding(fileName);
			var logFile = Create(fileName, encoding);
			_taskScheduler.RunOnce();

			var entries = GetEntries(logFile);
			entries.Count.Should().Be(actualLines.Count);
			for (int i = 0; i < actualLines.Count; ++i)
			{
				var entry = entries[i];
				var actualContent = actualLines[i];
				if (actualContent.Length > 0)
				{
					entry.RawContent.Should().Be(actualContent, $"because line {i+1} should have been read in correctly");
				}
				else
				{
					entry.RawContent.Should().BeNullOrEmpty($"because line {i+1} should have been read in correctly");
				}
			}
		}

		[Test]
		public void TestRead_AccessRandom_Ascending([ValueSource(nameof(ReferenceFiles))] string fileName)
		{
			var actualLines = File.ReadAllText(fileName).Split(new []{"\n"}, StringSplitOptions.None).Select(x => x.TrimEnd('\r')).ToList();

			var encoding = DetectEncoding(fileName);
			var logFile = Create(fileName, encoding);
			_taskScheduler.RunOnce();

			// Next, let us try random access
			for (int i = 0; i < actualLines.Count; i += 124)
			{
				var entry = GetEntry(logFile, i);
				var actualContent = actualLines[i];
				if (actualContent.Length > 0)
				{
					entry.RawContent.Should().Be(actualContent, $"because line {i+1} should have been read in correctly");
				}
				else
				{
					entry.RawContent.Should().BeNullOrEmpty($"because line {i+1} should have been read in correctly");
				}
			}
		}

		[Test]
		public void TestRead_AccessRandom_Descending([ValueSource(nameof(ReferenceFiles))] string fileName)
		{
			var actualLines = File.ReadAllText(fileName).Split(new []{"\n"}, StringSplitOptions.None).Select(x => x.TrimEnd('\r')).ToList();

			var encoding = DetectEncoding(fileName);
			var logFile = Create(fileName, encoding);
			_taskScheduler.RunOnce();

			// Next, let us try random access
			for (int i = actualLines.Count - 1; i >= 0; i -= 124)
			{
				var entry = GetEntry(logFile, i);
				var actualContent = actualLines[i];
				if (actualContent.Length > 0)
				{
					entry.RawContent.Should().Be(actualContent, $"because line {i+1} should have been read in correctly");
				}
				else
				{
					entry.RawContent.Should().BeNullOrEmpty($"because line {i+1} should have been read in correctly");
				}
			}
		}

		[Test]
		public void TestRead_Segmented_VerifyDiscardBuffer()
		{
			// Okay so here are the test conditions for this test to trigger the error case:
			// 1. We have to know the buffer size of StreamReader which seems to be 1024 bytes since ages
			// 2. We have to write data into the file and then request reads which fall onto different
			//    1024 byte boundaries into the source data
			// 3. We have to ensure that the data we read in line doesn't fill the entire buffer,
			//    but only parts of it, so that the next erroneously reads from the buffer first before
			//    then reading from the stream again.

			const int expectedStreamReaderBufferSize = 1024;
			var line1 = "that feeling when you bite into a pickle and it's a little squishier than you expected";
			var line2 = new string(' ', expectedStreamReaderBufferSize);
			var line3 = "Speedy gonzales";

			var encoding = Encoding.UTF8;
			var fileName = GetUniqueNonExistingFileName();
			using (var stream = File.OpenWrite(fileName))
			using (var writer = new StreamWriter(stream, encoding))
			{
				writer.WriteLine(line1);
				writer.WriteLine(line2);
				writer.WriteLine(line3);
			}

			var logFile = Create(fileName, encoding);
			_taskScheduler.RunOnce();

			var entries = GetEntries(logFile, new LogLineIndex[] {0, 2});
			entries[0].RawContent.Should().Be(line1);
			entries[1].RawContent.Should().Be(line3);
		}

		#endregion

		#endregion

		#region Dynamic Data

		[Test]
		public void TestClearFile()
		{
			var encoding = Encoding.UTF8;
			var fileName = GetUniqueNonExistingFileName();

			var logFile = Create(fileName, encoding);
			var changes = AddListener(logFile, 1000);
			changes.Should().Equal(new object[] {LogSourceModification.Reset()});

			using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
			using (var writer = new StreamWriter(stream, encoding))
			{
				writer.WriteLine("What is up people?");
				writer.Flush();
				_taskScheduler.RunOnce();

				var index = logFile.GetColumn(new LogSourceSection(0, 2), StreamingTextLogSource.LineOffsetInBytes);
				index[0].Should().Be(encoding.GetPreamble().Length);
				index[1].Should().Be(23);
				changes.Should().Equal(new object[] {LogSourceModification.Reset(), LogSourceModification.Appended(0, 1), LogSourceModification.Appended(1, 1)});

				var entries = GetEntries(logFile);
				entries.Count.Should().Be(2);
				entries[0].RawContent.Should().Be("What is up people?");
				entries[1].RawContent.Should().BeNullOrEmpty();
			}

			using (new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				_taskScheduler.RunOnce();

				logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(0, "because now we'Ve truncated the file which should have been detected by now");

				changes.Should().Equal(new object[] {LogSourceModification.Reset(), LogSourceModification.Appended(0, 1), LogSourceModification.Appended(1, 1), LogSourceModification.Reset()},
				                       "because the LogSource should have fired the Reset event now that the log source is done");

				var index = logFile.GetColumn(new LogSourceSection(0, 2), StreamingTextLogSource.LineOffsetInBytes);
				index[0].Should().Be(-1, "because now we'Ve truncated the file which should have been detected by now");
				index[1].Should().Be(-1, "because now we'Ve truncated the file which should have been detected by now");

				var entries = GetEntries(logFile, new LogSourceSection(0, 2));
				entries.Count.Should().Be(2);
				entries[0].RawContent.Should().BeNullOrEmpty();
				entries[1].RawContent.Should().BeNullOrEmpty();
			}
		}

		[Test]
		public void TestDeleteFile()
		{
			var encoding = Encoding.UTF8;
			var fileName = GetUniqueNonExistingFileName();

			var logFile = Create(fileName, encoding);

			using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
			using (var writer = new StreamWriter(stream, encoding))
			{
				writer.WriteLine("What is up people?");
				writer.Flush();
				_taskScheduler.RunOnce();

				var index = logFile.GetColumn(new LogSourceSection(0, 2), StreamingTextLogSource.LineOffsetInBytes);
				index[0].Should().Be(encoding.GetPreamble().Length);
				index[1].Should().Be(23);

				var entries = GetEntries(logFile);
				entries.Count.Should().Be(2);
				entries[0].RawContent.Should().Be("What is up people?");
				entries[1].RawContent.Should().BeNullOrEmpty();
			}

			File.Delete(fileName);

			{
				_taskScheduler.RunOnce();

				logFile.GetProperty(TextProperties.LineCount).Should().Be(0, "because now we've deleted the file which should have been detected by now");
				logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(0, "because now we've deleted the file which should have been detected by now");
				logFile.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist);

				var index = logFile.GetColumn(new LogSourceSection(0, 2), StreamingTextLogSource.LineOffsetInBytes);
				index[0].Should().Be(-1, "because now we've deleted the file which should have been detected by now");
				index[1].Should().Be(-1, "because now we've deleted the file which should have been detected by now");

				var entries = GetEntries(logFile, new LogSourceSection(0, 2));
				entries.Count.Should().Be(2);
				entries[0].RawContent.Should().BeNullOrEmpty();
				entries[1].RawContent.Should().BeNullOrEmpty();
			}
		}

		[Test]
		public void TestTail_WriteTwoLines()
		{
			var encoding = Encoding.UTF32;
			var fileName = GetUniqueNonExistingFileName();
			var logFile = Create(fileName, encoding);
			_taskScheduler.RunOnce();
			logFile.GetProperty(GeneralProperties.LogEntryCount).Should()
			       .Be(0, "because the file doesn't even exist yet");

			var line1 = "The sky crawlers";
			var line2 = "is awesome!";
			using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
			using (var writer = new StreamWriter(stream, encoding))
			{
				writer.Write(line1 + "\r\n");
				writer.Flush();
				_taskScheduler.RunOnce();

				var index = logFile.GetColumn(new LogSourceSection(0, 2), StreamingTextLogSource.LineOffsetInBytes);
				index[0].Should().Be(encoding.GetPreamble().Length);
				index[1].Should().Be(76);

				var entries = GetEntries(logFile);
				entries.Count.Should().Be(2);
				entries[0].RawContent.Should().Be(line1);
				entries[1].RawContent.Should().BeNullOrEmpty();


				writer.Write(line2 + "\r\n");
				writer.Flush();
				_taskScheduler.RunOnce();

				index = logFile.GetColumn(new LogSourceSection(0, 3), StreamingTextLogSource.LineOffsetInBytes);
				index[0].Should().Be(encoding.GetPreamble().Length);
				index[1].Should().Be(76);
				index[2].Should().Be(128);

				entries = GetEntries(logFile);
				entries.Count.Should().Be(3);
				entries[0].RawContent.Should().Be(line1);
				entries[1].RawContent.Should().Be(line2);
				entries[2].RawContent.Should().BeNullOrEmpty();
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/324")]
		public void TestTail_WriteTwoLines_Changes1()
		{
			var encoding = Encoding.UTF8;
			var fileName = GetUniqueNonExistingFileName();

			var line1 = "The sky crawlers";
			var line2 = "is awesome!";
			using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
			using (var writer = new StreamWriter(stream, encoding))
			using (var logFile = Create(fileName, encoding))
			{
				var changes = AddListener(logFile, 1000);
				changes.Should().Equal(new object[] {LogSourceModification.Reset()});

				writer.Write(line1 + "\r\n");
				writer.Flush();
				_taskScheduler.RunOnce();

				changes.Should().Equal(new object[] {LogSourceModification.Reset(), LogSourceModification.Appended(0, 1), LogSourceModification.Appended(1, 1)},
				                       "because the file consists of two lines, one being totally empty");

				writer.Write(line2 + "\r\n");
				writer.Flush();
				_taskScheduler.RunOnce();

				changes.Should().Equal(new object[] {LogSourceModification.Reset(), LogSourceModification.Appended(0, 1), LogSourceModification.Appended(1, 1), LogSourceModification.Removed(1, 1), LogSourceModification.Appended(1, 2)});
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/324")]
		public void TestTail_WriteTwoLines_Changes2()
		{
			var encoding = Encoding.UTF8;
			var fileName = GetUniqueNonExistingFileName();

			var line1 = "The sky crawlers";
			var line2 = "is awesome!";
			using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
			using (var writer = new StreamWriter(stream, encoding))
			using (var logFile = Create(fileName, encoding))
			{
				var changes = AddListener(logFile, 1000);
				changes.Should().Equal(new object[] {LogSourceModification.Reset()});

				writer.Write(line1);
				writer.Flush();
				_taskScheduler.RunOnce();

				changes.Should().Equal(new object[] {LogSourceModification.Reset(), LogSourceModification.Appended(0, 1)},
				                       "because the file consists of one line");

				writer.Write("\n" + line2);
				writer.Flush();
				_taskScheduler.RunOnce();

				//changes.Should().Equal(new object[] {LogSourceModification.Reset(), new LogFileSection(0, 1), new LogFileSection(1, 1)});
				// The current behavior won't cause wrong behavior in upstream listeners, but it will cause them unnecessary work.
				changes.Should().Equal(new object[] {LogSourceModification.Reset(), LogSourceModification.Appended(0, 1),
					LogSourceModification.Removed(0, 1), LogSourceModification.Appended(0, 2)});
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/324")]
		public void TestTail_WriteOneLineTwoFlushes_Changes3()
		{
			var encoding = Encoding.UTF8;
			var fileName = GetUniqueNonExistingFileName();

			var linePart1 = "The sky";
			var linePart2 = " Crawlers";
			using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
			using (var writer = new StreamWriter(stream, encoding))
			using (var logFile = Create(fileName, encoding))
			{
				var changes = AddListener(logFile, 1000);
				changes.Should().Equal(new object[] {LogSourceModification.Reset()});

				writer.Write(linePart1);
				writer.Flush();
				_taskScheduler.RunOnce();

				changes.Should().Equal(new object[] {LogSourceModification.Reset(), LogSourceModification.Appended(0, 1)},
				                       "because the file consists of one line");

				writer.Write(linePart2);
				writer.Flush();
				_taskScheduler.RunOnce();

				changes.Should().Equal(new object[] {LogSourceModification.Reset(), LogSourceModification.Appended(0, 1), LogSourceModification.Removed(0, 1), LogSourceModification.Appended(0, 1)});
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/324")]
		public void TestTail_WriteLinesClearWriteLines()
		{
			var encoding = new UTF8Encoding(false);
			var fileName = GetUniqueNonExistingFileName();

			using (var logFile = Create(fileName, encoding))
			{
				var line1 = "The sky";
				var line2 = "Crawlers";
				using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
				using (var writer = new StreamWriter(stream, encoding))
				{
					var changes = AddListener(logFile, 1000);
					changes.Should().Equal(new object[] {LogSourceModification.Reset()});

					writer.WriteLine(line1);
					writer.Flush();
					_taskScheduler.RunOnce();
					logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(2);
					var index = logFile.GetColumn(new LogSourceSection(0, 2), StreamingTextLogSource.LineOffsetInBytes);
					index[0].Should().Be(0);
					index[1].Should().Be(9);

					writer.WriteLine(line2);
					writer.Flush();
					_taskScheduler.RunOnce();
					logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(3);
					index = logFile.GetColumn(new LogSourceSection(0, 3), StreamingTextLogSource.LineOffsetInBytes);
					index[0].Should().Be(0);
					index[1].Should().Be(9);
					index[2].Should().Be(19);
				}

				using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
				using (var writer = new StreamWriter(stream, encoding))
				{
					writer.WriteLine("A");
					writer.WriteLine("B");
					writer.Flush();

					_taskScheduler.RunOnce();
					logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(3);
					var index = logFile.GetColumn(new LogSourceSection(0, 3), StreamingTextLogSource.LineOffsetInBytes);
					index[0].Should().Be(0);
					index[1].Should().Be(3);
					index[2].Should().Be(6);
				}
			}
		}

		#endregion

		private Encoding DetectEncoding(string fileName)
		{
			var detector = new EncodingDetector();
			return detector.TryFindEncoding(fileName) ?? Encoding.UTF8;
		}
	}
}
