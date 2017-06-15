using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using FluentAssertions;
using NUnit.Framework;

namespace WpfUnit.Test
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class TestMouseTest
	{
		[Test]
		public void TestMove1()
		{
			var mouse = new TestMouse();
			var control = new TestControl();

			control.Position.Should().Be(new Point(0, 0));
			mouse.MoveRelativeTo(control, new Point(42, 24));
			control.Position.Should().Be(new Point(42, 24));
		}
	}

	public sealed class TestControl
		: UIElement
	{
		public TestControl()
		{
			MouseMove += OnMouseMove;
		}

		private void OnMouseMove(object sender, MouseEventArgs args)
		{
			Position = Mouse.GetPosition(this);
		}

		public Point Position;
	}
}