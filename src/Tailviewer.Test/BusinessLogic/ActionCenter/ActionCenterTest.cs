using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.ActionCenter;

namespace Tailviewer.Test.BusinessLogic.ActionCenter
{
	[TestFixture]
	public sealed class ActionCenterTest
	{
		private Tailviewer.BusinessLogic.ActionCenter.ActionCenter _center;

		[SetUp]
		public void Setup()
		{
			_center = new Tailviewer.BusinessLogic.ActionCenter.ActionCenter();
		}

		[Test]
		[Description("Verifies that the center doesn't hold more than the maximum amount of notifications")]
		public void TestAdd1()
		{
			const int max = Tailviewer.BusinessLogic.ActionCenter.ActionCenter.MaximumNotificationCount;
			for (int i = 0; i < max; ++i)
			{
				_center.Add(new Mock<INotification>().Object);
				_center.Notifications.Count().Should().Be(i + 1);
			}

			_center.Add(new Mock<INotification>().Object);
			_center.Notifications.Count().Should().Be(max);

			_center.Add(new Mock<INotification>().Object);
			_center.Notifications.Count().Should().Be(max);
		}
	}
}