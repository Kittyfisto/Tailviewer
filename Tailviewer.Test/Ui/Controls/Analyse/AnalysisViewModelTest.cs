using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui.Controls.MainPanel.Analyse;

namespace Tailviewer.Test.Ui.Controls.Analyse
{
	[TestFixture]
	public sealed class AnalysisViewModelTest
	{
		[Test]
		public void TestCtor()
		{
			var model = new AnalysisViewModel();
			model.Pages.Should().NotBeNull();
			model.Pages.Should().HaveCount(1);
			model.Pages.First().Should().NotBeNull();
			model.Pages.First().DeletePageCommand.CanExecute(null).Should().BeFalse("because the last page may never be deleted");
		}

		[Test]
		public void TestAddPage()
		{
			var model = new AnalysisViewModel();
			model.Pages.Should().HaveCount(1);
			model.AddPageCommand.Execute(null);
			model.Pages.Should().HaveCount(2);
			model.Pages.ElementAt(0).DeletePageCommand.CanExecute(null).Should().BeTrue();
			model.Pages.ElementAt(1).DeletePageCommand.CanExecute(null).Should().BeTrue();
		}
	}
}
