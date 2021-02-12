using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Test;

namespace Tailviewer.AcceptanceTests.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class SingleDataSourceManualTest
	{
		private ManualTaskScheduler _scheduler;
		private string _fname;
		private FileStream _stream;
		private StreamWriter _writer;
		private TextLogFile _logFile;
		private DataSource _settings;

		[SetUp]
		public void SetUp()
		{
			_fname = Path.GetTempFileName();
			if (File.Exists(_fname))
				File.Delete(_fname);
			_scheduler = new ManualTaskScheduler();
			
			_stream = File.Open(_fname, FileMode.Create, FileAccess.Write, FileShare.Read);
			_writer = new StreamWriter(_stream);
			_logFile = Create(_fname);

			_settings = new DataSource(_fname)
			{
				Id = DataSourceId.CreateNew()
			};
		}

		private TextLogFile Create(string fileName)
		{
			var serviceContainer = new ServiceContainer();
			serviceContainer.RegisterInstance<ITaskScheduler>(_scheduler);
			serviceContainer.RegisterInstance<ILogFileFormatMatcher>(new SimpleLogFileFormatMatcher(LogFileFormats.GenericText));
			serviceContainer.RegisterInstance<ITextLogFileParserPlugin>(new SimpleTextLogFileParserPlugin());
			return new TextLogFile(serviceContainer, fileName);
		}

		[Test]
		[Description("Verifies that a line written to a file is correctly sent to the filtered log file")]
		public void TestWrite1([Values(true, false)] bool isSingleLine)
		{
			_settings.IsSingleLine = isSingleLine;
			using (var dataSource = new SingleDataSource(_scheduler, _settings, _logFile, TimeSpan.Zero))
			{
				_writer.Write("ssss");
				_writer.Flush();

				_scheduler.Run(2);
				dataSource.FilteredLogFile.Count.Should().Be(1);
				var line = dataSource.FilteredLogFile.GetEntry(0);
				line.Index.Should().Be(0);
				line.LogEntryIndex.Should().Be(0);
				line.RawContent.Should().Be("ssss");
				line.LogLevel.Should().Be(LevelFlags.Other);
			}
		}

		[Test]
		[Description("Verifies that a line written to a file is correctly sent to the filtered log file")]
		public void TestWrite2([Values(true, false)] bool isSingleLine)
		{
			_settings.IsSingleLine = isSingleLine;
			using (var dataSource = new SingleDataSource(_scheduler, _settings, _logFile, TimeSpan.Zero))
			{
				_writer.Write("Hello World\r\n");
				_writer.Flush();

				_scheduler.Run(2);
				dataSource.FilteredLogFile.Count.Should().Be(1);
				var line = dataSource.FilteredLogFile.GetEntry(0);
				line.Index.Should().Be(0);
				line.LogEntryIndex.Should().Be(0);
				line.RawContent.Should().Be("Hello World");
				line.LogLevel.Should().Be(LevelFlags.Other);
			}
		}

		[Test]
		[Description("Verifies that when a file is reset, then so is the filtered log file")]
		public void TestWrite3([Values(true, false)] bool isSingleLine)
		{
			_settings.IsSingleLine = isSingleLine;
			using (var dataSource = new SingleDataSource(_scheduler, _settings, _logFile, TimeSpan.Zero))
			{
				_writer.Write("Hello World\r\n");
				_writer.Flush();

				_scheduler.Run(2);
				dataSource.FilteredLogFile.Count.Should().Be(1);

				_stream.SetLength(0);
				_stream.Flush();

				_scheduler.Run(2);
				dataSource.FilteredLogFile.Count.Should().Be(0, "because the file on disk has been reset to a length of 0");
			}
		}

		[Test]
		[Description("Verifies that a line written in three separate flushes is correctly assembly to a single log line")]
		public void TestReadOneLine3([Values(true, false)] bool isSingleLine)
		{
			_settings.IsSingleLine = isSingleLine;
			using (var dataSource = new SingleDataSource(_scheduler, _settings, _logFile, TimeSpan.Zero))
			{
				_writer.Write("A");
				_writer.Flush();
				_scheduler.Run(2);

				_writer.Write("B");
				_writer.Flush();
				_scheduler.Run(2);

				_writer.Write("C");
				_writer.Flush();
				_scheduler.Run(2);

				dataSource.FilteredLogFile.Count.Should().Be(1, "because only a single line has been written to disk");
				var line = dataSource.FilteredLogFile.GetEntry(0);
				line.Index.Should().Be(0);
				line.LogEntryIndex.Should().Be(0);
				line.RawContent.Should().Be("ABC");
				line.LogLevel.Should().Be(LevelFlags.Other);
				line.Timestamp.Should().Be(null);
			}
		}

		[Test]
		[Description("Verifies that a multi line entry is correctly read into memory")]
		public void TestReadMultiline()
		{
			_settings.IsSingleLine = false;
			using (var dataSource = new SingleDataSource(_scheduler, _settings, _logFile, TimeSpan.Zero))
			{
				_writer.WriteLine("2015-10-07 19:50:58,981 INFO Starting");
				_writer.WriteLine("the application...");
				_writer.Flush();
				_scheduler.Run(2);

				dataSource.FilteredLogFile.Count.Should().Be(2, "because two lines have been written to the file");

				var t = new DateTime(2015, 10, 7, 19, 50, 58, 981);
				var line1 = dataSource.FilteredLogFile.GetEntry(0);
				line1.Index.Should().Be(0);
				line1.LogEntryIndex.Should().Be(0);
				line1.RawContent.Should().Be("2015-10-07 19:50:58,981 INFO Starting");
				line1.LogLevel.Should().Be(LevelFlags.Info);
				line1.Timestamp.Should().Be(t);

				var line2 = dataSource.FilteredLogFile.GetEntry(1);
				line2.Index.Should().Be(1);
				line2.LogEntryIndex.Should().Be(0);
				line2.RawContent.Should().Be("the application...");
				line2.LogLevel.Should().Be(LevelFlags.Info);
				line2.Timestamp.Should().Be(t);
			}
		}

		[Test]
		[Description("Verifies that a single line entry is correctly read into memory")]
		public void TestReadSingleLine()
		{
			_settings.IsSingleLine = true;
			using (var dataSource = new SingleDataSource(_scheduler, _settings, _logFile, TimeSpan.Zero))
			{
				_writer.WriteLine("2015-10-07 19:50:58,981 INFO Starting");
				_writer.WriteLine("the application...");
				_writer.Flush();
				_scheduler.Run(2);

				dataSource.FilteredLogFile.Count.Should().Be(2, "because two lines have been written to the file");

				var t = new DateTime(2015, 10, 7, 19, 50, 58, 981);
				var line1 = dataSource.FilteredLogFile.GetEntry(0);
				line1.Index.Should().Be(0);
				line1.LogEntryIndex.Should().Be(0);
				line1.RawContent.Should().Be("2015-10-07 19:50:58,981 INFO Starting");
				line1.LogLevel.Should().Be(LevelFlags.Info);
				line1.Timestamp.Should().Be(t);

				var line2 = dataSource.FilteredLogFile.GetEntry(1);
				line2.Index.Should().Be(1);
				line2.LogEntryIndex.Should().Be(1);
				line2.RawContent.Should().Be("the application...");
				line2.LogLevel.Should().Be(LevelFlags.Other);
				line2.Timestamp.Should().Be(null);
			}
		}
	}
}