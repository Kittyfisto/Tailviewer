using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Analysis.DataSources.BusinessLogic;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Analysis.DataSources.Test.BusinessLogic
{
	[TestFixture]
	public sealed class DataSourcesAnalyserTest
	{
		[Test]
		public void TestNoLogFile()
		{
			var analyser = new DataSourcesAnalyser(AnalyserId.Empty);
			analyser.Progress.Should().Be(Percentage.HundredPercent);

			analyser.Result.Should().NotBeNull();
			analyser.Result.Should().BeOfType<DataSourcesResult>();
			var result = (DataSourcesResult) analyser.Result;
			result.DataSources.Should().BeEmpty();
		}

		[Test]
		public void TestOneLogFile()
		{
			var logFile = new InMemoryLogFile();
			logFile.SetValue(LogFileProperties.Name, "Hello there.txt");

			var analyser = new DataSourcesAnalyser(AnalyserId.Empty);
			analyser.Progress.Should().Be(Percentage.HundredPercent);

			analyser.OnLogFileAdded(logFile);
			analyser.Result.Should().NotBeNull();
			analyser.Result.Should().BeOfType<DataSourcesResult>();
			var result = (DataSourcesResult)analyser.Result;
			result.DataSources.Should().HaveCount(1);
			result.DataSources[0].Name.Should().Be("Hello there.txt");
		}
	}
}
