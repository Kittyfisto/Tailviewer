using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Ui.Controls.MainPanel.Analyse;

namespace Tailviewer.Test.Ui.Controls.Analyse
{
	[TestFixture]
	public sealed class AnalysisViewModelTest
	{
		private Mock<IAnalyserGroup> _analyser;

		[SetUp]
		public void Setup()
		{
			_analyser = new Mock<IAnalyserGroup>();
		}

		[Test]
		public void TestCtor()
		{
			var model = new AnalysisViewModel(_analyser.Object);
			model.Pages.Should().NotBeNull();
			model.Pages.Should().HaveCount(1);
			model.Pages.First().Should().NotBeNull();
			model.Pages.First().DeletePageCommand.CanExecute(null).Should().BeFalse("because the last page may never be deleted");
		}

		[Test]
		public void TestAddPage1()
		{
			var model = new AnalysisViewModel(_analyser.Object);
			model.Pages.Should().HaveCount(1);
			model.AddPageCommand.Execute(null);
			model.Pages.Should().HaveCount(2);
			model.Pages.ElementAt(0).DeletePageCommand.CanExecute(null).Should().BeTrue();
			model.Pages.ElementAt(1).DeletePageCommand.CanExecute(null).Should().BeTrue();
		}

		[Test]
		public void TestAddPage2()
		{
			var model = new AnalysisViewModel(_analyser.Object);
			model.Template.Pages.Should().HaveCount(1);

			model.AddPageCommand.Execute(null);
			model.Template.Pages.Should().HaveCount(2);
			model.Template.Pages.ElementAt(1).Should().BeSameAs(
				model.Pages.ElementAt(1).Template
			);
		}
	}
}
