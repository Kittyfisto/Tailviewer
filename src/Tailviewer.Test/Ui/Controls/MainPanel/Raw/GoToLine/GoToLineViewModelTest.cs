using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui.Controls.MainPanel.Raw.GoToLine;

namespace Tailviewer.Test.Ui.Controls.MainPanel.Raw.GoToLine
{
	[TestFixture]
	public sealed class GoToLineViewModelTest
	{
		[Test]
		public void TestChooseLineNumber1()
		{
			var viewModel = new GoToLineViewModel();
			viewModel.MonitorEvents();

			viewModel.LineNumber.Should().BeNull();
			new Action(() => viewModel.ChoseLineNumber()).ShouldNotThrow();
			viewModel.ShouldNotRaise(nameof(GoToLineViewModel.LineNumberChosen),
				"because no line number was entered yet");
		}

		[Test]
		public void TestChooseLineNumber2()
		{
			var viewModel = new GoToLineViewModel();
			viewModel.MonitorEvents();

			viewModel.LineNumber = 42;
			new Action(() => viewModel.ChoseLineNumber()).ShouldNotThrow();
			viewModel.ShouldRaise(nameof(GoToLineViewModel.LineNumberChosen));
		}

		[Test]
		public void TestShow1()
		{
			var viewModel = new GoToLineViewModel();
			viewModel.Show = true;
			viewModel.LineNumber = 42;
			viewModel.LineNumber.Should().Be(42);
			viewModel.Show = false;

			viewModel.Show = true;
			viewModel.LineNumber.Should().BeNull("because any previous input should've been removed");
		}
	}
}