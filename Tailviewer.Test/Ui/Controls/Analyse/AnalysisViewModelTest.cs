using System.Linq;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.Ui.Controls.MainPanel.Analyse;

namespace Tailviewer.Test.Ui.Controls.Analyse
{
	[TestFixture]
	public sealed class AnalysisViewModelTest
	{
		private ManualDispatcher _dispatcher;
		private Mock<IAnalysis> _analyser;
		private AnalysisTemplate _template;
		private Mock<IAnalysisStorage> _analysisStorage;

		[SetUp]
		public void Setup()
		{
			_dispatcher = new ManualDispatcher();
			_template = new AnalysisTemplate();
			_analyser = new Mock<IAnalysis>();
			_analysisStorage = new Mock<IAnalysisStorage>();
		}

		[Test]
		public void TestCtor()
		{
			var model = new AnalysisViewModel(_dispatcher, _template, _analyser.Object, _analysisStorage.Object);
			model.Pages.Should().NotBeNull();
			model.Pages.Should().HaveCount(1);
			model.Pages.First().Should().NotBeNull();
			model.Pages.First().DeletePageCommand.CanExecute(null).Should().BeFalse("because the last page may never be deleted");
		}

		[Test]
		public void TestAddPage1()
		{
			var model = new AnalysisViewModel(_dispatcher, _template, _analyser.Object, _analysisStorage.Object);
			model.Pages.Should().HaveCount(1);
			model.AddPageCommand.Execute(null);
			model.Pages.Should().HaveCount(2);
			model.Pages.ElementAt(0).DeletePageCommand.CanExecute(null).Should().BeTrue();
			model.Pages.ElementAt(1).DeletePageCommand.CanExecute(null).Should().BeTrue();
		}

		[Test]
		public void TestAddPage2()
		{
			var model = new AnalysisViewModel(_dispatcher, _template, _analyser.Object, _analysisStorage.Object);
			model.Template.Pages.Should().HaveCount(1);

			model.AddPageCommand.Execute(null);
			model.Template.Pages.Should().HaveCount(2);
			model.Template.Pages.ElementAt(1).Should().BeSameAs(
				model.Pages.ElementAt(1).Template
			);
		}

		[Test]
		public void TestRemovePage1()
		{
			var model = new AnalysisViewModel(_dispatcher, _template, _analyser.Object, _analysisStorage.Object);
			model.Template.Pages.Should().HaveCount(1);

			model.AddPageCommand.Execute(null);
			model.Template.Pages.Should().HaveCount(2);
			model.Pages.ElementAt(1).DeletePageCommand.Execute(null);

			model.Template.Pages.Should().HaveCount(1);
		}
	}
}
