using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.Templates.Analysis;
using Tailviewer.Ui.Analysis;
using Tailviewer.Ui.Controls.MainPanel.Analyse;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts;

namespace Tailviewer.Test.Ui.Controls.Analyse
{
	[TestFixture]
	public sealed class AnalysisPageViewModelTest
	{
		private Mock<IAnalysis> _analyser;
		private PageTemplate _template;
		private Mock<IAnalysisStorage> _analysisStorage;
		private AnalysisId _id;
		private PluginRegistry _pluginRegistry;

		[SetUp]
		public void Setup()
		{
			_id = AnalysisId.CreateNew();
			_template = new PageTemplate();
			_analyser = new Mock<IAnalysis>();

			_analysisStorage = new Mock<IAnalysisStorage>();

			_pluginRegistry = new PluginRegistry();
		}

		[Test]
		public void TestConstruction()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			model.Name.Should().NotBeEmpty();
			model.PageLayout.Should().Be(PageLayout.WrapHorizontal);
			model.Layout.Should().NotBeNull();
			model.Layout.Should().BeOfType<HorizontalWidgetLayoutViewModel>();
		}

		[Test]
		public void TestConstructionFromNonEmptyTemplate()
		{
			_template.Layout = new HorizontalWidgetLayoutTemplate();
			var plugin = CreateWidgetPlugin();
			_pluginRegistry.Register(plugin);

			var analyser = AddAnalyser();

			_template.Add(new WidgetTemplate
			{
				AnalyserId = analyser.Id,
				LogAnalyserFactoryId = plugin.AnalyserId
			});

			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			model.Name.Should().NotBeEmpty();
			model.HasWidgets.Should().BeTrue("because we've created this page using a template with one widget");

			model.PageLayout.Should().Be(PageLayout.WrapHorizontal);
			model.Layout.Should().NotBeNull();
			model.Layout.Should().BeOfType<HorizontalWidgetLayoutViewModel>();
			var layout = (HorizontalWidgetLayoutViewModel)model.Layout;
			layout.Widgets.Should().HaveCount(1);

			_template.Widgets.Should().HaveCount(1, "because the page template musn't have been modified");
			_analysisStorage.Verify(x => x.Save(It.IsAny<AnalysisId>()), Times.Never,
			                        "because we've just create a view model from an existing template and NOT made any changes to said template. Therefore nothing should've been saved to disk");
		}

		[Test]
		[Description("Verifies that if a layout requests that a widget be added, the page does so")]
		public void TestRequestAddWidget1()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			var layout = (HorizontalWidgetLayoutViewModel) model.Layout;

			var widget = new Mock<IWidgetViewModel>().Object;
			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
				.Returns(widget);

			layout.Widgets.Should().NotContain(widget);

			layout.RaiseRequestAdd(factory.Object);
			layout.Widgets.Should().Contain(widget);
		}

		[Test]
		public void TestRequestAddWidget2()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);

			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
				.Returns((IWidgetTemplate template, IDataSourceAnalyser analyser) =>
				{
					var widget = new Mock<IWidgetViewModel>();
					widget.Setup(x => x.Template).Returns(template);
					return widget.Object;
				});

			var layout = (HorizontalWidgetLayoutViewModel)model.Layout;
			layout.RaiseRequestAdd(factory.Object);

			_template.Widgets.Should().HaveCount(1);
			_template.Widgets.First().Should().NotBeNull();
		}

		[Test]
		public void TestRequestAddWidget3()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			var layout = (HorizontalWidgetLayoutViewModel)model.Layout;

			var widget = new Mock<IWidgetViewModel>().Object;
			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
				.Returns(widget);

			layout.RaiseRequestAdd(factory.Object);

			model.PageLayout = PageLayout.None;
			model.PageLayout = PageLayout.WrapHorizontal;

			model.Layout.Should().NotBeSameAs(layout);
			((HorizontalWidgetLayoutViewModel)model.Layout).Widgets.Should().Contain(widget);
		}

		[Test]
		[Description("Verifies that once a widget is added, the analysis is saved once more")]
		public void TestRequestAddWidget4()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			var layout = (HorizontalWidgetLayoutViewModel)model.Layout;

			var widget = new Mock<IWidgetViewModel>().Object;
			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
			       .Returns(widget);


			_analysisStorage.Verify(x => x.Save(It.IsAny<AnalysisId>()), Times.Never);

			layout.RaiseRequestAdd(factory.Object);
			_analysisStorage.Verify(x => x.Save(_id), Times.Once,
			                        "because the page should've saved the analysis now that a widget's been added");
		}

		[Test]
		public void TestRemoveWidget1()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			var layout = (HorizontalWidgetLayoutViewModel)model.Layout;

			var widget = new Mock<IWidgetViewModel>();
			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
				.Returns(widget.Object);
			
			layout.RaiseRequestAdd(factory.Object);
			layout.Widgets.Should().Contain(widget.Object);

			widget.Raise(x => x.OnDelete += null, widget.Object);
			layout.Widgets.Should().BeEmpty();
			_analyser.Verify(x => x.Remove(It.IsAny<IDataSourceAnalyser>()), Times.Once,
				"because the analyser created with that widget should've been removed again");
		}

		[Test]
		public void TestRemoveWidget2()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			var layout = (HorizontalWidgetLayoutViewModel)model.Layout;

			var widget = new Mock<IWidgetViewModel>();
			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
			       .Returns(widget.Object);

			_analysisStorage.Verify(x => x.Save(It.IsAny<AnalysisId>()), Times.Never);

			layout.RaiseRequestAdd(factory.Object);
			_analysisStorage.Verify(x => x.Save(_id), Times.Once);

			layout.Widgets.Should().Contain(widget.Object);
			widget.Raise(x => x.OnDelete += null, widget.Object);
			_analysisStorage.Verify(x => x.Save(_id), Times.Exactly(2),
			                        "because the page should've saved the analysis now that a widget's been removed");
		}

		private IWidgetPlugin CreateWidgetPlugin()
		{
			var widget = new Mock<IWidgetViewModel>().Object;
			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
			       .Returns(widget);
			factory.Setup(x => x.AnalyserId).Returns(new LogAnalyserFactoryId("Test Widget Plugin"));
			return factory.Object;
		}

		private IDataSourceAnalyser AddAnalyser()
		{
			var dataSourceAnalyser = new Mock<IDataSourceAnalyser>();
			var analyserId = AnalyserId.CreateNew();
			dataSourceAnalyser.Setup(x => x.Id).Returns(analyserId);
			var analyser = dataSourceAnalyser.Object;

			_analyser.Setup(x => x.TryGetAnalyser(analyserId, out analyser)).Returns(true);

			return analyser;
		}
	}
}