using System;
using System.Windows.Documents;
using NUnit.Framework;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class DragLayerTest
	{
		private AdornerLayer _layer;
		private AdornerDecorator _decorator;

		[SetUp]
		[STAThread]
		public void SetUp()
		{
			_decorator = new AdornerDecorator();
			_layer = _decorator.AdornerLayer;
		}

		[Test]
		[STAThread]
		public void TestDoDragDrop1()
		{
			
		}
	}
}