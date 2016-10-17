using System.Linq;
using FluentAssertions;
using Metrolib;
using NUnit.Framework;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.Ui.ViewModels.ActionCenter;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class ActionCenterViewModelTest
	{
		private ManualDispatcher _dispatcher;
		private IActionCenter _actionCenter;
		private ActionCenterViewModel _viewModel;

		[SetUp]
		public void Setup()
		{
			_dispatcher = new ManualDispatcher();
			_actionCenter = new ActionCenter();
			_viewModel = new ActionCenterViewModel(_dispatcher, _actionCenter);
		}

		[Test]
		public void TestAdd2()
		{
			_viewModel.Notifications.Should().BeEmpty();
			_actionCenter.Add(Notification.CreateInfo("Foo", "Hello World!"));
			_dispatcher.InvokeAll();
			_viewModel.Notifications.Count().Should().Be(1);
			_viewModel.UnreadCount.Should().Be(1);
			_viewModel.HasNewMessages.Should().BeTrue();
		}

		[Test]
		public void TestAdd3()
		{
			var notification1 = Notification.CreateInfo("Foo", "Hello World!");
			var notification2 = Notification.CreateInfo("Bar", "Clondyke Bar");
			_actionCenter.Add(notification1);
			_actionCenter.Add(notification2);
			_dispatcher.InvokeAll();
			_viewModel.Notifications.Count().Should().Be(2);
			_viewModel.Notifications.ElementAt(0).Title.Should().Be("Bar");
			_viewModel.Notifications.ElementAt(1).Title.Should().Be("Foo");
		}
	}
}