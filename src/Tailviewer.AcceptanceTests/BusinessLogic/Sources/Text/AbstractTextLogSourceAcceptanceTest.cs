﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using FluentAssertions;
using log4net;
using Moq;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Plugins;
using Tailviewer.Test;

namespace Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text
{
	[TestFixture]
	public abstract class AbstractTextLogSourceAcceptanceTest
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public const string File1Mb_1Line = @"TestData\1Mb_1Line.txt";
		public const string File1Mb_2Lines = @"TestData\1Mb_2Lines.txt";
		public const string File2Mb = @"TestData\2Mb.txt";
		public const string File20Mb = @"TestData\20Mb.txt";
		public const string File2Lines = @"TestData\2Lines.txt";
		public const string File2Entries = @"TestData\2LogEntries.txt";
		public const string FileTestLive1 = @"TestData\TestLive1.txt";
		public const string FileTestLive2 = @"TestData\TestLive2.txt";
		public const string MultilineNoLogLevel1 = @"TestData\Multiline\Log1.txt";
		public const string MultilineNoLogLevel2 = @"TestData\Multiline\Log2.txt";

		private ServiceContainer _serviceContainer;
		private DefaultTaskScheduler _taskScheduler;
		private SimpleLogFileFormatMatcher _formatMatcher;
		private SimpleLogEntryParserPlugin _logEntryParserPlugin;

		[SetUp]
		public void SetUp()
		{
			_serviceContainer = new ServiceContainer();
			_taskScheduler = new DefaultTaskScheduler();
			_formatMatcher = new SimpleLogFileFormatMatcher(LogFileFormats.GenericText);
			_logEntryParserPlugin = new SimpleLogEntryParserPlugin();

			_serviceContainer.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_serviceContainer.RegisterInstance<ILogFileFormatMatcher>(_formatMatcher);
			_serviceContainer.RegisterInstance<ILogEntryParserPlugin>(_logEntryParserPlugin);
		}

		[TearDown]
		public void TearDown()
		{
			_taskScheduler.Dispose();
		}

		protected abstract ILogSource Create(ITaskScheduler taskScheduler, string fileName, ILogFileFormat format, Encoding encoding);

		protected ILogSource Create(string fileName, ILogFileFormat format, Encoding encoding)
		{
			return Create(_taskScheduler, fileName, format, encoding);
		}

		protected ILogSource Create(string fileName, Encoding encoding)
		{
			return Create(_taskScheduler, fileName, LogFileFormats.GenericText, encoding);
		}

		protected ILogSource Create(string fileName)
		{
			return Create(fileName, Encoding.Default);
		}

		private string GetUniqueNonExistingFileName()
		{
			var fileName = PathEx.GetTempFileName();
			if (File.Exists(fileName))
				File.Delete(fileName);

			TestContext.WriteLine("FileName: {0}", fileName);
			return fileName;
		}

		[Test]
		[Description("Verifies that the format property is immediately visible after construction")]
		public void TestConstruction()
		{
			using (var logFile = Create("", LogFileFormats.Csv, Encoding.Default))
			{
				logFile.GetProperty(GeneralProperties.Format).Should().Be(LogFileFormats.Csv);
			}
		}

		[Test]
		public void TestNotifyListenersEventually()
		{
			var line = "Hello, World!";
			var fileName = GetUniqueNonExistingFileName();

			using (var handle = new ManualResetEvent(false))
			{
				var logFile = Create(fileName, Encoding.Default);
				var listener = new Mock<ILogSourceListener>();
				listener.Setup(x => x.OnLogFileModified(logFile, new LogFileSection(0, 1)))
				        .Callback(() => handle.Set());
				logFile.AddListener(listener.Object, TimeSpan.FromMilliseconds(100), 2);

				using (var stream = File.OpenWrite(fileName))
				using (var writer = new StreamWriter(stream, Encoding.Default))
				{
					writer.Write(line);
				}

				handle.WaitOne(TimeSpan.FromSeconds(2))
				      .Should().BeTrue("because the listener should eventually (after ~100ms) be notified that one line was added");
			}
		}

		[Test]
		public void TestDelete1()
		{
			const string fname = "TestDelete1.log";
			File.WriteAllText(fname, "Test");

			using (var logFile = Create(fname))
			{
				logFile.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);

				new Action(() =>
					{
						for (int i = 0; i < 10; ++i)
						{
							try
							{
								File.Delete(fname);
								return;
							}
							catch (IOException)
							{
							}
						}

						File.Delete(fname);
					}).Should().NotThrow();
			}
		}
		
		[Test]
		[Description("Verifies that creating a LogFile for a file that doesn't exist works")]
		public void TestDoesNotExist()
		{
			ILogSource logSource = null;
			try
			{
				new Action(() => logSource = Create( "dadwdawdw")).Should().NotThrow();

				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				logSource.Property(x => x.GetProperty(GeneralProperties.EmptyReason)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(ErrorFlags.SourceDoesNotExist);

				logSource.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist, "Because the specified file doesn't exist");
			}
			finally
			{
				if (logSource != null)
					logSource.Dispose();
			}
		}

		[Test]
		public void TestExists()
		{
			ILogSource logSource = null;
			try
			{
				new Action(() => logSource = Create( File2Lines)).Should().NotThrow();

				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				logSource.Property(x => x.GetProperty(GeneralProperties.EmptyReason)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(ErrorFlags.None);

				logSource.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.None, "Because the specified file does exist");
			}
			finally
			{
				if (logSource != null)
					logSource.Dispose();
			}
		}

		[Test]
		public void TestLive1()
		{
			string fname = GetUniqueNonExistingFileName();

			using (var logger = new FileLogger(fname))
			using (var logFile = Create(fname))
			{
				logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(0);

				Log.Info("Test");

				logFile.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).BeGreaterOrEqualTo(1);
				var entries = logFile.GetEntries();
				entries[0].RawContent.Should().EndWith("Test");
			}
		}

		[Test]
		public void TestLive2()
		{
			string fname = GetUniqueNonExistingFileName();

			using (var logger = new FileLogger(fname))
			using (var logFile = Create(fname))
			{
				logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(0);

				Log.Info("Hello");
				logFile.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).BeGreaterOrEqualTo(1);

				Log.Info("world!");
				logFile.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).BeGreaterOrEqualTo(2);
			}
		}

		[Test]
		[Description("Verifies that opening a log file before the file is created works and that its contents can be read")]
		public void TestOpenBeforeCreate()
		{
			ILogSource logSource = null;
			try
			{
				string fileName = PathEx.GetTempFileName();
				if (File.Exists(fileName))
					File.Delete(fileName);

				new Action(() => logSource = Create(fileName)).Should().NotThrow();

				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				logSource.Property(x => x.GetProperty(GeneralProperties.EmptyReason)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(ErrorFlags.SourceDoesNotExist,
				                                                           "Because the specified file doesn't exist");

				File.WriteAllText(fileName, "Hello World!");

				logSource.Property(x => x.GetProperty(GeneralProperties.EmptyReason)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(ErrorFlags.None,
				                                                          "Because the file has been created now");
				logSource.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1, "Because one line was written to the file");

				var entry = logSource.GetEntry(0);
				entry.Index.Should().Be(0);
				entry.RawContent.Should().Be("Hello World!");
			}
			finally
			{
				if (logSource != null)
					logSource.Dispose();
			}
		}

		[Test]
		public void TestReadAll2()
		{
			using (var file = Create(File20Mb))
			{
				var listener = new Mock<ILogSourceListener>();
				var sections = new List<LogFileSection>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogFileSection>()))
				        .Callback((ILogSource logFile, LogFileSection section) => sections.Add(section));

				file.AddListener(listener.Object, TimeSpan.Zero, 1);

				file.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(20)).Be(Percentage.HundredPercent);
				file.GetProperty(GeneralProperties.LogEntryCount).Should().Be(165342);

				sections[0].Should().Equal(LogFileSection.Reset);
				for (int i = 1; i < sections.Count; ++i)
				{
					LogFileSection change = sections[i];
					change.Index.Should().Be((LogLineIndex) (i - 1));
					change.Count.Should().Be(1);
				}
			}
		}

		[Test]
		[Description("Verifies that the maximum number of characters for all lines is determined correctly")]
		public void TestReadAll3()
		{
			using (var file = Create(File20Mb))
			{
				file.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(20)).Be(Percentage.HundredPercent);

				file.GetProperty(GeneralProperties.LogEntryCount).Should().Be(165342);
			}
		}
	}
}