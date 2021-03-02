using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Ui.QuickNavigation;

namespace Tailviewer.Tests.Ui.Controls.QuickNavigation
{
	[TestFixture]
	public sealed class DataSourceSuggestionViewModelTest
	{
		[Test]
		public void TestEquality1()
		{
			var viewModel = new DataSourceSuggestionViewModel(new Mock<IDataSource>().Object, null, null, null);
			viewModel.Equals(viewModel).Should().BeTrue();
			viewModel.GetHashCode().Should().Be(viewModel.GetHashCode());
		}

		[Test]
		public void TestEquality2()
		{
			var viewModel = new DataSourceSuggestionViewModel(new Mock<IDataSource>().Object, null, null, null);
			viewModel.Equals(null).Should().BeFalse();
		}

		[Test]
		public void TestEquality3()
		{
			var viewModel1 = new DataSourceSuggestionViewModel(new Mock<IDataSource>().Object, null, null, null);
			var viewModel2 = new DataSourceSuggestionViewModel(new Mock<IDataSource>().Object, null, null, null);
			viewModel1.Equals(viewModel2).Should().BeFalse();
			viewModel2.Equals(viewModel1).Should().BeFalse();
		}
	}
}