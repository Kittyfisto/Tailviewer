using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.AcceptanceTests.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.AcceptanceTests.Ui.ViewModels
{
	[TestFixture]
	public sealed class SingleDataSourceViewModelTest
	{
		private DefaultTaskScheduler _taskScheduler;

		[SetUp]
		public void SetUp()
		{
			_taskScheduler = new DefaultTaskScheduler();
		}

		[Test]
		[Description("Verifies that once the search has results, the view model automatically marks and jumps to the first occurence")]
		public void TestSearch1()
		{
			var settings = new DataSource(LogFileTest.File2Mb) { Id = Guid.NewGuid() };
			using (var logFile = new LogFile(_taskScheduler, LogFileTest.File2Mb))
			using (var dataSource = new SingleDataSource(_taskScheduler, settings, logFile, TimeSpan.Zero))
			{
				var model = new SingleDataSourceViewModel(dataSource);

				logFile.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();

				model.Update();
				model.TotalCount.Should().Be(16114);

				model.SearchTerm = "RPC #12";
				var search = dataSource.Search;
				search.Property(x => x.Count).ShouldEventually().Be(334);

				model.Update();
				model.SearchResultCount.Should().Be(334);
				model.CurrentSearchResultIndex.Should().Be(0);
				model.SelectedLogLines.Should().Equal(new[] {new LogLineIndex(44)});
				model.VisibleLogLine.Should().Be(new LogLineIndex(44));
			}
		}
	}
}