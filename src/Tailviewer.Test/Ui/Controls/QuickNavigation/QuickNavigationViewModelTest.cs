using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Settings;
using Tailviewer.Ui.QuickNavigation;

namespace Tailviewer.Test.Ui.Controls.QuickNavigation
{
	[TestFixture]
	public sealed class QuickNavigationViewModelTest
	{
		private Mock<IDataSources> _dataSources;
		private List<IDataSource> _sources;

		[SetUp]
		public void Setup()
		{
			_dataSources = new Mock<IDataSources>();
			_sources = new List<IDataSource>();
			_dataSources.Setup(x => x.Sources).Returns(_sources);
		}

		[Test]
		[Description("Verifies that suggestions are updated once the search term is changed")]
		public void TestSearch1()
		{
			var viewModel = new QuickNavigationViewModel(_dataSources.Object);
			_sources.Add(CreateSingleDataSource("Tailviewer.log"));

			viewModel.Suggestions.Should().BeNullOrEmpty();
			viewModel.SearchTerm = "TAIL";
			viewModel.Suggestions.Should().HaveCount(1);
		}

		[Test]
		[Description("Verifies that the suggestion contains the original path with the original case, not the one of the search term")]
		public void TestSearch2()
		{
			var viewModel = new QuickNavigationViewModel(_dataSources.Object);
			_sources.Add(CreateSingleDataSource("Tailviewer.log"));

			viewModel.SearchTerm = "AIL";
			var suggestion = viewModel.Suggestions[0];
			suggestion.Should().NotBeNull();
			suggestion.Prefix.Should().Be("T");
			suggestion.Midfix.Should().Be("ail");
			suggestion.Postfix.Should().Be("viewer.log");
		}

		[Test]
		[Description("Verifies that no suggestion is created when nothing matches")]
		public void TestSearch3()
		{
			var viewModel = new QuickNavigationViewModel(_dataSources.Object);
			_sources.Add(CreateSingleDataSource("A"));
			_sources.Add(CreateSingleDataSource("AA"));

			viewModel.SearchTerm = "B";
			viewModel.Suggestions.Should().BeNullOrEmpty();
			viewModel.SelectedSuggestion.Should().BeNull();
		}

		[Test]
		[Description("Verifies that the first suggestion is selected by default")]
		public void TestSearch4()
		{
			var viewModel = new QuickNavigationViewModel(_dataSources.Object);
			_sources.Add(CreateSingleDataSource("A"));
			_sources.Add(CreateSingleDataSource("AA"));

			viewModel.SearchTerm = "A";
			viewModel.Suggestions.Should().HaveCount(2);
			viewModel.SelectedSuggestion.Should().Be(viewModel.Suggestions[0]);
		}

		[Test]
		[Description("Verifies that the merged data source is found")]
		public void TestSearch5()
		{
			var viewModel = new QuickNavigationViewModel(_dataSources.Object);
			
			_sources.Add(CreateMergedDataSource("Foobar"));
			_sources.Add(CreateSingleDataSource("Foo"));

			viewModel.SearchTerm = "Foo";
			viewModel.Suggestions.Should().HaveCount(2);
		}

		[Test]
		public void TestChooseSuggestion()
		{
			var viewModel = new QuickNavigationViewModel(_dataSources.Object);
			_sources.Add(CreateSingleDataSource("A"));
			_sources.Add(CreateSingleDataSource("AA"));

			viewModel.SearchTerm = "AA";
			viewModel.Suggestions.Should().HaveCount(1);
			var suggestion = viewModel.Suggestions[0];
			viewModel.ChooseDataSourceCommand.CanExecute(suggestion).Should().BeTrue();

			using (var monitor = viewModel.Monitor())
			{
				viewModel.ChooseDataSourceCommand.Execute(suggestion);
				monitor.Should().Raise(nameof(QuickNavigationViewModel.DataSourceChosen));
			}
		}

		private static IDataSource CreateSingleDataSource(string fileName)
		{
			var dataSource = new Mock<IDataSource>();
			dataSource.Setup(x => x.FullFileName).Returns(fileName);
			return dataSource.Object;
		}

		private static IDataSource CreateMergedDataSource(string sourceName)
		{
			
			var dataSource = new Mock<IDataSource>();
			dataSource.Setup(x => x.Settings).Returns(new DataSource {DisplayName = sourceName});
			return dataSource.Object;
		}
	}
}