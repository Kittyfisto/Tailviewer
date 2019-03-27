using System.Threading;
using System.Windows;
using System.Windows.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Ui.Controls.LogView;
using WpfUnit;

namespace Tailviewer.AcceptanceTests.Ui.Controls.LogView
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class LogEntryListViewTest
	{
		private LogEntryListView _control;

		[SetUp]
		public void SetUp()
		{
			_control = new LogEntryListView
			{
				Width = 1024,
				Height = 768
			};
		}

		[Test]
		[Description("Verifies that the control starts its dispatcher timer when loaded and stops it when unloaded")]
		public void TestLoadUnload()
		{
			var dispatcher = Dispatcher.CurrentDispatcher;
			dispatcher.GetActiveDispatcherTimers().Should().BeEmpty("because the first timer should be started when the control is loaded, NOT before");

			_control.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
			dispatcher.GetActiveDispatcherTimers().Should().HaveCount(1);

			_control.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
			dispatcher.GetActiveDispatcherTimers().Should().BeEmpty();

			_control.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
			dispatcher.GetActiveDispatcherTimers().Should().HaveCount(1);

			_control.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
			dispatcher.GetActiveDispatcherTimers().Should().BeEmpty();
		}

		[Test]
		[Description("Verifies that multiple LoadedEvents in succession do not cause further timers to be created")]
		public void TestLoad1()
		{
			var dispatcher = Dispatcher.CurrentDispatcher;

			_control.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
			try
			{
				dispatcher.GetActiveDispatcherTimers().Should().HaveCount(1);

				_control.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
				dispatcher.GetActiveDispatcherTimers().Should().HaveCount(1, "because there should still only be one timer");
			}
			finally
			{
				_control.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
			}
		}
	}
}