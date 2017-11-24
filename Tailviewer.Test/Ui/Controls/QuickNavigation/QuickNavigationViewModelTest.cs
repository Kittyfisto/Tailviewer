using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Ui.Controls.QuickNavigation;

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
		public void TestSearch1()
		{
			var viewModel = new QuickNavigationViewModel(_dataSources.Object);
			_sources.Add(CreateDataSource("Tailviewer.log"));

			viewModel.Suggestions.Should().BeNullOrEmpty();
			viewModel.SearchTerm = "TAIL";
			viewModel.Suggestions.Should().HaveCount(1);
		}

		[Test]
		public void TestSearch2()
		{
			var viewModel = new QuickNavigationViewModel(_dataSources.Object);
			_sources.Add(CreateDataSource("Tailviewer.log"));

			viewModel.SearchTerm = "AIL";
			var suggestion = viewModel.Suggestions[0];
			suggestion.Should().NotBeNull();
			suggestion.Prefix.Should().Be("T");
			suggestion.Midfix.Should().Be("ail");
			suggestion.Postfix.Should().Be("viewer.log");
		}

		private static IDataSource CreateDataSource(string fileName)
		{
			var dataSource = new Mock<IDataSource>();
			dataSource.Setup(x => x.FullFileName).Returns(fileName);
			return dataSource.Object;
		}
	}
}