using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.Core.Analysis.Layouts;
using Tailviewer.Templates.Analysis;
using Tailviewer.Ui.Analysis;
using Tailviewer.Ui.Controls.MainPanel.Analyse;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts.Column;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts.HorizontalWrap;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts.Row;

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
			_template = new PageTemplate{Title = "A page", Layout = new HorizontalWidgetLayoutTemplate()};
			_analyser = new Mock<IAnalysis>();

			_analysisStorage = new Mock<IAnalysisStorage>();

			_pluginRegistry = new PluginRegistry();
		}

		[Test]
		public void TestConstructionEmptyTemplate()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			model.Name.Should().NotBeEmpty();
			model.PageLayout.Should().Be(PageLayout.WrapHorizontal);
			model.Layout.Should().NotBeNull();
			model.Layout.Should().BeOfType<HorizontalWrapWidgetLayoutViewModel>();
			((HorizontalWrapWidgetLayoutViewModel) model.Layout).Widgets.Should().BeEmpty();
			model.HasWidgets.Should().BeFalse();
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
				AnalyserPluginId = plugin.AnalyserId
			});

			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			model.Name.Should().NotBeEmpty();
			model.HasWidgets.Should().BeTrue("because we've created this page using a template with one widget");

			model.PageLayout.Should().Be(PageLayout.WrapHorizontal);
			model.Layout.Should().NotBeNull();
			model.Layout.Should().BeOfType<HorizontalWrapWidgetLayoutViewModel>();
			var layout = (HorizontalWrapWidgetLayoutViewModel)model.Layout;
			layout.Widgets.Should().HaveCount(1);

			_template.Widgets.Should().HaveCount(1, "because the page template musn't have been modified");
			_analysisStorage.Verify(x => x.SaveAsync(It.IsAny<AnalysisId>()), Times.Never,
			                        "because we've just create a view model from an existing template and NOT made any changes to said template. Therefore nothing should've been saved to disk");
		}

		[Test]
		public void TestConstructionFromColumnLayoutTemplate()
		{
			_template.Layout = new ColumnWidgetLayoutTemplate();
			var plugin = CreateWidgetPlugin();
			_pluginRegistry.Register(plugin);

			var analyser = AddAnalyser();

			_template.Add(new WidgetTemplate
			{
				AnalyserId = analyser.Id,
				AnalyserPluginId = plugin.AnalyserId
			});

			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			model.Name.Should().NotBeEmpty();
			model.HasWidgets.Should().BeTrue("because we've created this page using a template with one widget");

			model.PageLayout.Should().Be(PageLayout.Columns);
			model.Layout.Should().NotBeNull();
			model.Layout.Should().BeOfType<ColumnWidgetLayoutViewModel>();
			var layout = (ColumnWidgetLayoutViewModel)model.Layout;
			layout.Widgets.Should().HaveCount(1);

			_template.Widgets.Should().HaveCount(1, "because the page template musn't have been modified");
			_analysisStorage.Verify(x => x.SaveAsync(It.IsAny<AnalysisId>()), Times.Never,
				"because we've just create a view model from an existing template and NOT made any changes to said template. Therefore nothing should've been saved to disk");
		}

		[Test]
		public void TestConstructionFromRowLayoutTemplate()
		{
			_template.Layout = new RowWidgetLayoutTemplate();
			var plugin = CreateWidgetPlugin();
			_pluginRegistry.Register(plugin);

			var analyser = AddAnalyser();

			_template.Add(new WidgetTemplate
			{
				AnalyserId = analyser.Id,
				AnalyserPluginId = plugin.AnalyserId
			});

			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			model.Name.Should().NotBeEmpty();
			model.HasWidgets.Should().BeTrue("because we've created this page using a template with one widget");

			model.PageLayout.Should().Be(PageLayout.Rows);
			model.Layout.Should().NotBeNull();
			model.Layout.Should().BeOfType<RowWidgetLayoutViewModel>();
			var layout = (RowWidgetLayoutViewModel)model.Layout;
			layout.Widgets.Should().HaveCount(1);

			_template.Widgets.Should().HaveCount(1, "because the page template musn't have been modified");
			_analysisStorage.Verify(x => x.SaveAsync(It.IsAny<AnalysisId>()), Times.Never,
				"because we've just create a view model from an existing template and NOT made any changes to said template. Therefore nothing should've been saved to disk");
		}

		[Test]
		public void TestConstructionFromTemplateWithoutAnalyzer()
		{
			_template.Layout = new HorizontalWidgetLayoutTemplate();
			var plugin = CreateWidgetPlugin();
			_pluginRegistry.Register(plugin);

			_template.Add(new WidgetTemplate
			{
				AnalyserId = AnalyserId.Empty,
				AnalyserPluginId = plugin.AnalyserId
			});

			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			model.Name.Should().NotBeEmpty();
			model.HasWidgets.Should().BeTrue("because we've created this page using a template with one widget");

			model.PageLayout.Should().Be(PageLayout.WrapHorizontal);
			model.Layout.Should().NotBeNull();
			model.Layout.Should().BeOfType<HorizontalWrapWidgetLayoutViewModel>();
			var layout = (HorizontalWrapWidgetLayoutViewModel)model.Layout;
			layout.Widgets.Should().HaveCount(1);

			_template.Widgets.Should().HaveCount(1, "because the page template musn't have been modified");
			_analysisStorage.Verify(x => x.SaveAsync(It.IsAny<AnalysisId>()), Times.Never,
			                        "because we've just create a view model from an existing template and NOT made any changes to said template. Therefore nothing should've been saved to disk");
		}

		[Test]
		[Description("Verifies that if a layout requests that a widget be added, the page does so")]
		public void TestRequestAddWidget1()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			var layout = (HorizontalWrapWidgetLayoutViewModel) model.Layout;

			var widget = new Mock<IWidgetViewModel>().Object;
			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
				.Returns(widget);

			layout.Widgets.Should().NotContain(x => x.InnerViewModel == widget);

			layout.RaiseRequestAdd(factory.Object);
			layout.Widgets.Should().Contain(x => x.InnerViewModel == widget);
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

			var layout = (HorizontalWrapWidgetLayoutViewModel)model.Layout;
			layout.RaiseRequestAdd(factory.Object);

			_template.Widgets.Should().HaveCount(1);
			_template.Widgets.First().Should().NotBeNull();
		}

		[Test]
		public void TestRequestAddWidget3()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			var layout = (HorizontalWrapWidgetLayoutViewModel)model.Layout;

			var widget = new Mock<IWidgetViewModel>().Object;
			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
				.Returns(widget);

			layout.RaiseRequestAdd(factory.Object);

			model.PageLayout = PageLayout.None;
			model.PageLayout = PageLayout.WrapHorizontal;

			model.Layout.Should().NotBeSameAs(layout);
			((HorizontalWrapWidgetLayoutViewModel)model.Layout).Widgets.Should().Contain(x => x.InnerViewModel == widget);
		}

		[Test]
		[Description("Verifies that once a widget is added, the analysis is saved once more")]
		public void TestRequestAddWidget4()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			var layout = (HorizontalWrapWidgetLayoutViewModel)model.Layout;

			var widget = new Mock<IWidgetViewModel>().Object;
			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
			       .Returns(widget);


			_analysisStorage.Verify(x => x.SaveAsync(It.IsAny<AnalysisId>()), Times.Never);

			layout.RaiseRequestAdd(factory.Object);
			_analysisStorage.Verify(x => x.SaveAsync(_id), Times.Once,
			                        "because the page should've saved the analysis now that a widget's been added");
		}

		[Test]
		public void TestRemoveWidget1()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			var layout = (HorizontalWrapWidgetLayoutViewModel)model.Layout;

			var widget = new Mock<IWidgetViewModel>();
			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
				.Returns(widget.Object);

			layout.RaiseRequestAdd(factory.Object);
			var viewModel = layout.Widgets.FirstOrDefault(x => x.InnerViewModel == widget.Object);
			viewModel.Should().NotBeNull();
			viewModel.DeleteCommand.Execute(null);

			layout.Widgets.Should().BeEmpty();
			_analyser.Verify(x => x.Remove(It.IsAny<IDataSourceAnalyser>()), Times.Once,
				"because the analyser created with that widget should've been removed again");
		}

		[Test]
		public void TestRemoveWidget2()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			var layout = (HorizontalWrapWidgetLayoutViewModel)model.Layout;

			var widget = new Mock<IWidgetViewModel>();
			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
			       .Returns(widget.Object);

			_analysisStorage.Verify(x => x.SaveAsync(It.IsAny<AnalysisId>()), Times.Never);

			layout.RaiseRequestAdd(factory.Object);
			_analysisStorage.Verify(x => x.SaveAsync(_id), Times.Once);

			var viewModel = layout.Widgets.FirstOrDefault(x => x.InnerViewModel == widget.Object);
			viewModel.Should().NotBeNull();
			viewModel.DeleteCommand.Execute(null);
			_analysisStorage.Verify(x => x.SaveAsync(_id), Times.Exactly(2),
			                        "because the page should've saved the analysis now that a widget's been removed");
		}

		[Test]
		[Description("Verifies that changing the layout is stored in the template")]
		public void TestChangeLayout()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);

			model.PageLayout = PageLayout.Columns;
			model.Layout.Should().BeOfType<ColumnWidgetLayoutViewModel>();
			_template.Layout.Should().BeOfType<ColumnWidgetLayoutTemplate>("because the template should have been modified");
			_analysisStorage.Verify(x => x.SaveAsync(It.Is<AnalysisId>(y => y == _id)), Times.Once);

			model.PageLayout = PageLayout.Rows;
			model.Layout.Should().BeOfType<RowWidgetLayoutViewModel>();
			_template.Layout.Should().BeOfType<RowWidgetLayoutTemplate>("because the template should have been modified");
			_analysisStorage.Verify(x => x.SaveAsync(It.Is<AnalysisId>(y => y == _id)), Times.Exactly(2));

			model.PageLayout = PageLayout.WrapHorizontal;
			model.Layout.Should().BeOfType<HorizontalWrapWidgetLayoutViewModel>();
			_template.Layout.Should().BeOfType<HorizontalWidgetLayoutTemplate>("because the template should have been modified");
			_analysisStorage.Verify(x => x.SaveAsync(It.Is<AnalysisId>(y => y == _id)), Times.Exactly(3));
		}

		[Test]
		[Description("Verifies that the page causes the analysis to be saved whenever a widget says it has modified the configuration")]
		public void TestOnTemplateModified()
		{
			var model = new AnalysisPageViewModel(_id, _template, _analyser.Object, _analysisStorage.Object, _pluginRegistry);
			var widget = AddWidget(model);

			_analysisStorage.Verify(x => x.SaveAsync(It.Is<AnalysisId>(y => y == _id)), Times.Once, "because we've added a new widget");

			widget.Raise(x => x.TemplateModified += null);
			_analysisStorage.Verify(x => x.SaveAsync(It.Is<AnalysisId>(y => y == _id)), Times.Exactly(2), "because the new widget has requested another save");

			widget.Raise(x => x.TemplateModified += null);
			_analysisStorage.Verify(x => x.SaveAsync(It.Is<AnalysisId>(y => y == _id)), Times.Exactly(3), "because the new widget has requested another save");
		}

		private Mock<IWidgetViewModel> AddWidget(AnalysisPageViewModel model)
		{
			var widget = new Mock<IWidgetViewModel>();
			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
				.Returns(widget.Object);

			var layout = (HorizontalWrapWidgetLayoutViewModel) model.Layout;
			layout.RaiseRequestAdd(factory.Object);

			return widget;
		}

		private IWidgetPlugin CreateWidgetPlugin()
		{
			var widget = new Mock<IWidgetViewModel>().Object;
			var factory = new Mock<IWidgetPlugin>();
			factory.Setup(x => x.CreateViewModel(It.IsAny<IWidgetTemplate>(), It.IsAny<IDataSourceAnalyser>()))
			       .Returns(widget);
			factory.Setup(x => x.AnalyserId).Returns(new AnalyserPluginId("Test Widget Plugin"));
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