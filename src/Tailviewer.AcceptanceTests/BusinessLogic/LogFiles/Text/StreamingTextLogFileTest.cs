using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Metrolib;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Core.IO;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.LogFiles.Text;
using Tailviewer.Test;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles.Text
{
	/// <summary>
	/// Tests <see cref="StreamingTextLogFile"/> together with an actual <see cref="IoScheduler"/>, reading log files from disk.
	/// </summary>
	[TestFixture]
	public sealed class StreamingTextLogFileTest
	{
		private ServiceContainer _serviceContainer;
		private ManualTaskScheduler _taskScheduler;
		private SimpleLogFileFormatMatcher _formatMatcher;
		private SimpleTextLogFileParserPlugin _textLogFileParserPlugin;
		
		[SetUp]
		public void SetUp()
		{
			_serviceContainer = new ServiceContainer();
			_taskScheduler = new ManualTaskScheduler();
			_formatMatcher = new SimpleLogFileFormatMatcher(LogFileFormats.GenericText);
			_textLogFileParserPlugin = new SimpleTextLogFileParserPlugin();

			_serviceContainer.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_serviceContainer.RegisterInstance<ILogFileFormatMatcher>(_formatMatcher);
			_serviceContainer.RegisterInstance<ITextLogFileParserPlugin>(_textLogFileParserPlugin);

		}

		private string GetUniqueNonExistingFileName()
		{
			var fileName = PathEx.GetTempFileName();
			if (File.Exists(fileName))
				File.Delete(fileName);

			TestContext.WriteLine("FileName: {0}", fileName);
			return fileName;
		}

		private StreamingTextLogFile Create(string fileName, Encoding encoding)
		{
			return new StreamingTextLogFile(_serviceContainer, fileName, encoding);
		}

		private IReadOnlyLogEntries GetEntries(StreamingTextLogFile logFile)
		{
			return GetEntries(logFile, new LogFileSection(0, logFile.GetProperty(Properties.LogEntryCount)));
		}

		private IReadOnlyLogEntries GetEntries(StreamingTextLogFile logFile, IReadOnlyList<LogLineIndex> sourceSection)
		{
			var readTask = Task.Factory.StartNew(() => logFile.GetEntries(sourceSection));
			logFile.Property(x => x.HasPendingReadRequests).ShouldEventually().BeTrue();
			_taskScheduler.RunOnce();
			readTask.Wait();
			//readTask.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue("because the task should have been finished now that we've let the scheduler run");
			return readTask.Result;
		}

		[Test]
		public void TestConstruction()
		{
			var logFile = Create(GetUniqueNonExistingFileName(), Encoding.Default);
			logFile.GetProperty(Properties.LogEntryCount).Should().Be(0);
			logFile.GetProperty(Properties.Size).Should().BeNull("because the log file didn't even have enough time to check the source");
			logFile.GetProperty(Properties.Created).Should().BeNull("because the log file didn't even have enough time to check the source");
			logFile.GetProperty(Properties.LastModified).Should().BeNull("because the log file didn't even have enough time to check the source");
			logFile.GetProperty(Properties.PercentageProcessed).Should().Be(Percentage.Zero);
			logFile.GetProperty(Properties.EmptyReason).Should().Be(ErrorFlags.None, "because the log file didn't have enough time to check the source");
		}

		#region Static data

		[Test]
		public void TestFileDoesNotExist()
		{
			var logFile = Create(GetUniqueNonExistingFileName(), Encoding.Default);
			_taskScheduler.RunOnce();

			logFile.GetProperty(Properties.LogEntryCount).Should().Be(0);
			logFile.GetProperty(Properties.Size).Should().BeNull("because the source file does not exist");
			logFile.GetProperty(Properties.Created).Should().BeNull("because the source file does not exist");
			logFile.GetProperty(Properties.LastModified).Should().BeNull("because the source file does not exist");
			logFile.GetProperty(Properties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist, "because the source file does not exist");
			logFile.GetProperty(Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because we've checked that the source doesn't exist and thus there's nothing more to process");
		}

		[Test]
		public void TestEmptyFile()
		{
			var fileName = @"TestData\Empty.txt";
			var info = FileFingerprint.FromFile(fileName);

			var logFile = Create(fileName, Encoding.Default);
			_taskScheduler.RunOnce();

			logFile.GetProperty(Properties.LogEntryCount).Should().Be(0, "because the file is empty");
			logFile.GetProperty(Properties.Size).Should().Be(Size.Zero, "because the file is empty");
			logFile.GetProperty(Properties.Created).Should().Be(info.Created);
			logFile.GetProperty(Properties.LastModified).Should().Be(info.LastModified);
			logFile.GetProperty(Properties.EmptyReason).Should().Be(ErrorFlags.None, "because the source file does exist and can be accessed");
			logFile.GetProperty(Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because we've checked that the source doesn't exist and thus there's nothing more to process");

			var indices = logFile.GetColumn(new LogFileSection(0, 1), Columns.LineOffsetInBytes);
			indices[0].Should().Be(Columns.LineOffsetInBytes.DefaultValue);

			var entries = GetEntries(logFile);
			entries.Should().BeEmpty();
		}

		[Test]
		public void Test1Line()
		{
			var fileName = @"TestData\1Line.txt";
			var info = FileFingerprint.FromFile(fileName);

			var logFile = Create(fileName, Encoding.Default);
			_taskScheduler.RunOnce();

			logFile.GetProperty(Properties.LogEntryCount).Should().Be(1);
			logFile.GetProperty(Properties.Size).Should().Be(Size.FromBytes(109));
			logFile.GetProperty(Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

			var indices = logFile.GetColumn(new LogFileSection(0, 1), Columns.LineOffsetInBytes);
			indices[0].Should().Be(0, "because the first line starts at an offset of 0 bytes wrt the start of the file");

			var entries = GetEntries(logFile);
			entries.Should().HaveCount(1);
			entries.Columns.Should().Equal(new IColumnDescriptor[]{Columns.Index, Columns.LineOffsetInBytes, Columns.RawContent});
			entries[0].Index.Should().Be(0);
			entries[0].RawContent.Should().Be(@"[00:00:01] git clone -q --branch=master https://github.com/Kittyfisto/SharpRemote.git C:\projects\sharpremote");
		}

		[Test]
		public void Test2Lines()
		{
			// TODO: Rename to 3 lines because that filename is just plain wrong
			var fileName = @"TestData\2Lines.txt";
			var info = FileFingerprint.FromFile(fileName);

			var logFile = Create(fileName, Encoding.Default);
			_taskScheduler.RunOnce();

			logFile.GetProperty(Properties.LogEntryCount).Should().Be(3);
			logFile.GetProperty(Properties.Size).Should().Be(Size.FromBytes(274));
			logFile.GetProperty(Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

			var indices = logFile.GetColumn(new LogFileSection(0, 3), Columns.LineOffsetInBytes);
			indices[0].Should().Be(0, "because the first line starts at an offset of 0 bytes wrt the start of the file");
			indices[1].Should().Be(166L);

			var entries = GetEntries(logFile);
			entries.Should().HaveCount(3);
			entries.Columns.Should().Equal(new IColumnDescriptor[]{Columns.Index, Columns.LineOffsetInBytes, Columns.RawContent});
			entries[0].Index.Should().Be(0);
			entries[0].RawContent.Should().Be("2015-10-07 19:50:58,981 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver");
			entries[1].Index.Should().Be(1);
			entries[1].RawContent.Should().Be("2015-10-07 19:50:58,997 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1");
		}

		#endregion
	}
}
