using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Api.Tests;
using Tailviewer.Core;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Sources.Buffer;
using Tailviewer.Core.Sources.Text.Simple;

namespace Tailviewer.Acceptance.Tests.BusinessLogic.Sources.Text.Simple
{
	[TestFixture]
	public sealed class TextLogSourceTest
	{
		private ManualTaskScheduler _taskScheduler;
		private string _fname;
		private FileStream _stream;
		private StreamWriter _streamWriter;
		private TextLogSource _source;
		private Mock<ILogSourceListener> _listener;
		private List<LogSourceModification> _modifications;
		private BinaryWriter _binaryWriter;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();
			_fname = PathEx.GetTempFileName();
			if (File.Exists(_fname))
				File.Delete(_fname);

			_stream = File.Open(_fname, FileMode.Create, FileAccess.Write, FileShare.Read);
			_streamWriter = new StreamWriter(_stream);
			_binaryWriter = new BinaryWriter(_stream);

			_source = Create(_fname);

			_listener = new Mock<ILogSourceListener>();
			_modifications = new List<LogSourceModification>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogSourceModification>()))
				.Callback((ILogSource unused, LogSourceModification modification) => _modifications.Add(modification));

			_source.AddListener(_listener.Object, TimeSpan.Zero, 10);
		}

		[TearDown]
		public void TearDown()
		{
			_binaryWriter?.Dispose();
			//_streamWriter?.Dispose();
			_stream?.Dispose();
		}

		private TextLogSource Create(string fileName)
		{
			return Create(fileName, Encoding.Default);
		}

		private TextLogSource Create(string fileName,
		                           Encoding encoding)
		{
			return new TextLogSource(_taskScheduler, fileName, LogFileFormats.GenericText, encoding);
		}

		[Test]
		public void TestConstruction()
		{
			_source.Columns.Should().Equal(new IColumnDescriptor[]
			{
				GeneralColumns.Index,
				GeneralColumns.OriginalIndex,
				GeneralColumns.LogEntryIndex,
				GeneralColumns.LineNumber,
				GeneralColumns.OriginalLineNumber,
				GeneralColumns.OriginalDataSourceName,
				GeneralColumns.RawContent,
				PageBufferedLogSource.RetrievalState
			});

			_streamWriter.Write("Foo");
			_streamWriter.Flush();

			var info = new FileInfo(_fname);
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.LastModified).Should().Be(info.LastWriteTimeUtc);
			_source.GetProperty(GeneralProperties.Created).Should().Be(info.CreationTimeUtc);
		}

		[Test]
		public void TestDoesNotExist()
		{
			_source = Create(_fname);
			_taskScheduler.RunOnce();
			_source.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.None);
			_source.GetProperty(GeneralProperties.Created).Should().NotBe(DateTime.MinValue);

			_streamWriter?.Dispose();
			_stream?.Dispose();
			File.Delete(_fname);
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist);
			_source.GetProperty(GeneralProperties.Created).Should().BeNull();
		}

		[Test]
		public void TestReadOneLine1()
		{
			_streamWriter.Write("Foo");
			_streamWriter.Flush();

			_taskScheduler.RunOnce();
			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);
			var entry = _source.GetEntry(0);
			entry.Index.Should().Be(0);
			entry.LogEntryIndex.Should().Be(0);
			entry.RawContent.Should().Be("Foo");
		}

		[Test]
		[Description("Verifies that a line written in two separate flushes is correctly assembly to a single log line")]
		public void TestReadOneLine2()
		{
			_streamWriter.Write("Hello ");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_streamWriter.Write("World!");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);
			var entry = _source.GetEntry(0);
			entry.Index.Should().Be(0);
			entry.LogEntryIndex.Should().Be(0);
			entry.RawContent.Should().Be("Hello World!");
		}

		[Test]
		[Description("Verifies that a line written in three separate flushes is correctly assembly to a single log line")]
		public void TestReadOneLine3()
		{
			_streamWriter.Write("A");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_streamWriter.Write("B");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_streamWriter.Write("C");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);
			var entry = _source.GetEntry(0);
			entry.Index.Should().Be(0);
			entry.LogEntryIndex.Should().Be(0);
			entry.RawContent.Should().Be("ABC");
		}

		[Test]
		[Description("Verifies that the correct sequence of modification is fired when a log line is assembled from many reads")]
		public void TestReadOneLine4()
		{
			_streamWriter.Write("A");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_streamWriter.Write("B");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_streamWriter.Write("C");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_modifications.Should().Equal(new object[]
			{
				LogSourceModification.Reset(),
				LogSourceModification.Appended(0, 1),
				LogSourceModification.Removed(0, 1),
				LogSourceModification.Appended(0, 1),
				LogSourceModification.Removed(0, 1),
				LogSourceModification.Appended(0, 1)
			}, "because the log file should've sent invalidations for the 2nd and 3rd read (because the same line was modified)");

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);
			var entry = _source.GetEntry(0);
			entry.Index.Should().Be(0);
			entry.LogEntryIndex.Should().Be(0);
			entry.RawContent.Should().Be("ABC");
		}

		[Test]
		public void TestReadTwoLines1()
		{
			_source.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.Zero);

			_streamWriter.Write("Hello\r\n");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();
			_source.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because the log file should have processed the entire file");

			_streamWriter.Write("World!\r\n");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();
			_source.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because the log file should have processed the entire file");

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(2);
			var entries = _source.GetEntries(new LogSourceSection(0, 2));
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].RawContent.Should().Be("Hello");
			
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(1);
			entries[1].RawContent.Should().Be("World!");
		}

		[Test]
		public void Test()
		{
			_source.GetProperty(GeneralProperties.Name).Should().Be(_fname);
		}
	}
}