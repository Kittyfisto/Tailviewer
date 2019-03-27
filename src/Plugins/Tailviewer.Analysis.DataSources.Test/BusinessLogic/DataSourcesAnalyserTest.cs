using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.LogFiles;
using Tailviewer.DataSources.BusinessLogic;

namespace Tailviewer.DataSources.Test.BusinessLogic
{
	[TestFixture]
	public sealed class DataSourcesAnalyserTest
	{
		[Test]
		public void TestEmptyLogFile()
		{
			var logFile = new InMemoryLogFile(LogFileColumns.OriginalDataSourceName);
			var analyser = new DataSourcesAnalyser(logFile, TimeSpan.Zero);
			analyser.Progress.Should().Be(Percentage.HundredPercent);

			analyser.Result.Should().NotBeNull();
			analyser.Result.Should().BeOfType<DataSourcesResult>();
			var result = (DataSourcesResult) analyser.Result;
			result.DataSources.Should().BeEmpty();
		}

		[Test]
		public void TestOneDataSource()
		{
			var logFile = new InMemoryLogFile(LogFileColumns.OriginalDataSourceName);

			var entry = new LogEntry2();
			entry.Add(LogFileColumns.OriginalDataSourceName, "Hello there.txt");
			logFile.Add(entry);

			var analyser = new DataSourcesAnalyser(logFile, TimeSpan.Zero);
			analyser.Progress.Should().Be(Percentage.HundredPercent);

			analyser.Result.Should().NotBeNull();
			analyser.Result.Should().BeOfType<DataSourcesResult>();
			var result = (DataSourcesResult)analyser.Result;
			result.DataSources.Should().HaveCount(1);
			result.DataSources[0].Name.Should().Be("Hello there.txt");
		}
	}
}
