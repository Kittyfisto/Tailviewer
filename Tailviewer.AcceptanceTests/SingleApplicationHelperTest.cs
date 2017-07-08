using Moq;
using NUnit.Framework;

namespace Tailviewer.AcceptanceTests
{
	[TestFixture]
	public sealed class SingleApplicationHelperTest
	{
		[Test]
		public void TestOpenFile1()
		{
			using (var mutex = SingleApplicationHelper.AcquireMutex())
			{
				var listener = new Mock<SingleApplicationHelper.IMessageListener>();
				mutex.SetListener(listener.Object);

				const string fname = @"F:\logs\lte.txt";
				SingleApplicationHelper.OpenFile(new[] {fname});
				listener.Verify(x => x.OnOpenDataSource(It.Is<string>(y => y == fname)), Times.Once);
			}
		}

		[Test]
		public void TestBringToFront()
		{
			using (var mutex = SingleApplicationHelper.AcquireMutex())
			{
				var listener = new Mock<SingleApplicationHelper.IMessageListener>();
				mutex.SetListener(listener.Object);

				SingleApplicationHelper.BringToFront();
				listener.Verify(x => x.OnShowMainwindow(), Times.Once);
			}
		}
	}
}