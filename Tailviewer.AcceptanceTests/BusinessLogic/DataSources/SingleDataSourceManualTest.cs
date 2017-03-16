using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.AcceptanceTests.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class SingleDataSourceManualTest
	{
		private ManualTaskScheduler _scheduler;

		[SetUp]
		public void SetUp()
		{
			_scheduler = new ManualTaskScheduler();
		}

		[Test]
		[Description("Verifies that a line written to a file is correctly sent to the filtered log file")]
		public void TestWrite1()
		{
			var fname = Path.GetTempFileName();
			if (File.Exists(fname))
				File.Delete(fname);

			var settings = new DataSource(fname)
			{
				Id = Guid.NewGuid()
			};
			using (var stream = File.Open(fname, FileMode.Create, FileAccess.Write, FileShare.Read))
			using (var writer = new StreamWriter(stream))
			using (var logFile = new LogFile(_scheduler, fname))
			using (var dataSource = new SingleDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				writer.Write("Hello World\r\n");
				writer.Flush();

				_scheduler.Run(2);
				dataSource.FilteredLogFile.Count.Should().Be(1);
				dataSource.FilteredLogFile.GetLine(0).Should().Be(new LogLine(0, 0, "Hello World", LevelFlags.None));
			}
		}

		[Test]
		[Description("Verifies that when a file is reset, then so is the filtered log file")]
		public void TestWrite2()
		{
			var fname = Path.GetTempFileName();
			if (File.Exists(fname))
				File.Delete(fname);

			var settings = new DataSource(fname)
			{
				Id = Guid.NewGuid()
			};
			using (var stream = File.Open(fname, FileMode.Create, FileAccess.Write, FileShare.Read))
			using (var writer = new StreamWriter(stream))
			using (var logFile = new LogFile(_scheduler, fname))
			using (var dataSource = new SingleDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				writer.Write("Hello World\r\n");
				writer.Flush();

				_scheduler.Run(2);
				dataSource.FilteredLogFile.Count.Should().Be(1);

				stream.SetLength(0);
				stream.Flush();

				_scheduler.Run(2);
				dataSource.FilteredLogFile.Count.Should().Be(0, "because the file on disk has been reset to a length of 0");
			}
		}
	}
}