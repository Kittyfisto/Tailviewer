using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Analysis.Count.Ui;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Analysis.Count.Test.Ui
{
	[TestFixture]
	public sealed class LogEntryCountWidgetViewModelTest
	{
		private Mock<IDataSourceAnalyser> _analyser;

		[SetUp]
		public void Setup()
		{
			_analyser = new Mock<IDataSourceAnalyser>();
		}

		[Test]
		[Description("Verifies that changing the title changes the template and causes the template modified event to be fired")]
		public void TestChangeTitle()
		{
			var template = new WidgetTemplate
			{};
			var model = new LogEntryCountWidgetViewModel(template, _analyser.Object);
			model.MonitorEvents();

			model.Title = "Hello there...";
			template.Title.Should().Be("Hello there...");
			model.ShouldRaise(nameof(IWidgetViewModel.TemplateModified));

			model.Title = "Sup!";
			template.Title.Should().Be("Sup!");
			model.ShouldRaise(nameof(IWidgetViewModel.TemplateModified));
		}

		[Test]
		[Description("Verifies that changing the caption changes the template and causes the template modified event to be fired")]
		public void TestChangeCaption()
		{
			var widgetConfiguration = new LogEntryCountWidgetConfiguration();
			var template = new WidgetTemplate
			{
				Configuration = widgetConfiguration
			};
			var model = new LogEntryCountWidgetViewModel(template, _analyser.Object);
			model.MonitorEvents();

			model.Caption = "Hello there...";
			widgetConfiguration.Caption.Should().Be("Hello there...");
			model.ShouldRaise(nameof(IWidgetViewModel.TemplateModified));

			model.Caption = "Sup!";
			widgetConfiguration.Caption.Should().Be("Sup!");
			model.ShouldRaise(nameof(IWidgetViewModel.TemplateModified));
		}
	}
}
