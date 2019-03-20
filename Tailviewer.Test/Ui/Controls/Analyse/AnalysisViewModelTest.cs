using System.Linq;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
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
		private AnalysisViewTemplate _viewTemplate;
		private Mock<IAnalysisStorage> _analysisStorage;
		private AnalysisId _id;
		private PluginRegistry _pluginRegistry;

		[SetUp]
		public void Setup()
		{
			_dispatcher = new ManualDispatcher();
			_viewTemplate = new AnalysisViewTemplate();
			_analyser = new Mock<IAnalysis>();
			_id = AnalysisId.CreateNew();
			_analyser.Setup(x => x.Id).Returns(_id);
			_analysisStorage = new Mock<IAnalysisStorage>();
			_pluginRegistry = new PluginRegistry();
		}

		[Test]
		public void TestCtorEmptyTemplate()
		{
			var model = new AnalysisViewModel(_dispatcher, _viewTemplate, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			model.Pages.Should().NotBeNull();
			model.Pages.Should().HaveCount(1);
			model.Pages.First().Should().NotBeNull();
			model.Pages.First().DeletePageCommand.CanExecute(null).Should().BeFalse("because the last page may never be deleted");
			model.Pages.First().Name.Should().Be("New Page");
		}

		[Test]
		public void TestCtorTemplateTwoPages()
		{
			_viewTemplate.Add(new PageTemplate{Title = "Page A"});
			_viewTemplate.Add(new PageTemplate{Title = "Page B"});

			var model = new AnalysisViewModel(_dispatcher, _viewTemplate, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			model.Pages.Should().NotBeNull();
			model.Pages.Should().HaveCount(2);
			model.Pages.First().Should().NotBeNull();
			model.Pages.First().Name.Should().Be("Page A");
			model.Pages.Last().Should().NotBeNull();
			model.Pages.Last().Name.Should().Be("Page B");
		}

		[Test]
		public void TestChangeName()
		{
			_viewTemplate.Name = "Some Name";
			var model = new AnalysisViewModel(_dispatcher, _viewTemplate, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			model.Name.Should().Be("Some Name");

			_analysisStorage.Verify(x => x.SaveAsync(It.IsAny<AnalysisId>()), Times.Never);

			model.Name = "Foobar";
			model.Name.Should().Be("Foobar");
			_viewTemplate.Name.Should().Be("Foobar");

			_analysisStorage.Verify(x => x.SaveAsync(_id), Times.Once);
		}

		[Test]
		public void TestAddPage1()
		{
			var model = new AnalysisViewModel(_dispatcher, _viewTemplate, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			model.Pages.Should().HaveCount(1);
			model.AddPageCommand.Execute(null);
			model.Pages.Should().HaveCount(2);
			model.Pages.ElementAt(0).DeletePageCommand.CanExecute(null).Should().BeTrue();
			model.Pages.ElementAt(1).DeletePageCommand.CanExecute(null).Should().BeTrue();
		}

		[Test]
		public void TestAddPage2()
		{
			var model = new AnalysisViewModel(_dispatcher, _viewTemplate, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			_viewTemplate.Pages.Should().HaveCount(1);

			model.AddPageCommand.Execute(null);
			_viewTemplate.Pages.Should().HaveCount(2);
			_viewTemplate.Pages.ElementAt(1).Should().BeSameAs(
				model.Pages.ElementAt(1).Template
			);
		}

		[Test]
		public void TestRemovePage1()
		{
			var model = new AnalysisViewModel(_dispatcher, _viewTemplate, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			_viewTemplate.Pages.Should().HaveCount(1);

			model.AddPageCommand.Execute(null);
			_viewTemplate.Pages.Should().HaveCount(2);
			model.Pages.ElementAt(1).DeletePageCommand.Execute(null);

			_viewTemplate.Pages.Should().HaveCount(1);
		}
	}
}
