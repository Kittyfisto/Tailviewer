using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Analysis.QuickInfo.BusinessLogic;
using Tailviewer.Analysis.QuickInfo.Ui;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Analysis.QuickInfo.Test.Ui
{
	[TestFixture]
	public sealed class QuickInfoWidgetViewModelTest
	{
		[Test]
		public void TestConstructionFromTemplate()
		{
			var id = Guid.NewGuid();
			var widgetConfiguration = new QuickInfoWidgetConfiguration();
			widgetConfiguration.Add(new QuickInfoViewConfiguration(id)
			{
				Name = "Cool entry"
			});
			var template = new WidgetTemplate
			{
				Configuration = widgetConfiguration
			};
			var analyser = new Mock<IDataSourceAnalyser>();
			var analyserConfiguration = new QuickInfoAnalyserConfiguration();
			analyserConfiguration.Add(new QuickInfoConfiguration(id)
			{
				FilterValue = "Foo"
			});
			analyser.Setup(x => x.Configuration).Returns(analyserConfiguration);

			var viewModel = new QuickInfoWidgetViewModel(template, analyser.Object);
			viewModel.QuickInfos.Should().HaveCount(1);
			var quickInfo = viewModel.QuickInfos.First();
			quickInfo.Should().NotBeNull();
			quickInfo.Id.Should().Be(id);
			quickInfo.Name.Should().Be("Cool entry");
			quickInfo.FilterValue.Should().Be("Foo");
		}

		[Test]
		public void TestAdd()
		{
			var widgetConfiguration = new QuickInfoWidgetConfiguration();
			var template = new WidgetTemplate
			{
				Configuration = widgetConfiguration
			};
			var analyser = new Mock<IDataSourceAnalyser>();
			var analyserConfiguration = new QuickInfoAnalyserConfiguration();
			analyser.Setup(x => x.Configuration).Returns(analyserConfiguration);
			var viewModel = new QuickInfoWidgetViewModel(template, analyser.Object);
			viewModel.MonitorEvents();

			viewModel.AddQuickInfoCommand.Execute(null);
			widgetConfiguration.Titles.Should().HaveCount(1, "because we've just added one new element");
			//analyserConfiguration.QuickInfos.Should().HaveCount(1, "because we've just added one new element");
			viewModel.ShouldRaise(nameof(QuickInfoWidgetViewModel.TemplateModified));
		}
	}
}
