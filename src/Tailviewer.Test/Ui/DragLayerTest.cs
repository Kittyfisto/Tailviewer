using System.Threading;
using System.Windows.Documents;
using NUnit.Framework;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public sealed class DragLayerTest
	{
		[SetUp]
		public void SetUp()
		{
			_decorator = new AdornerDecorator();
			_layer = _decorator.AdornerLayer;
		}

		private AdornerLayer _layer;
		private AdornerDecorator _decorator;

		[Test]
		public void TestDoDragDrop1()
		{
		}
	}
}