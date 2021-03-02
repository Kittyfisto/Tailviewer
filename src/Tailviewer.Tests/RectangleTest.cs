using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;

namespace Tailviewer.Tests
{
	[TestFixture]
	public sealed class RectangleTest
	{
		[Test]
		public void TestCutCompletelyInside()
		{
			var screen = new Desktop.Rectangle(0, 0, 10, 10);
			var window = new Desktop.Rectangle(4, 4, 2, 2);
			var cut = window.Cut(screen);
			cut.Should().BeEmpty("because the window is completely within the screen");
		}

		[Test]
		public void TestCutCompletelyOutside()
		{
			var screen = new Desktop.Rectangle(0, 0, 10, 10);
			var window = new Desktop.Rectangle(11, 11, 2, 2);
			var cut = window.Cut(screen);
			cut.Should().Equal(new[]{window},
			                      "because the screen doesn't cut anway anything from the window because the latter is complete off-screen");
		}

		[Test]
		public void TestCutIntersectLeft()
		{
			var screen = new Desktop.Rectangle(0, 0, 10, 10);
			var window = new Desktop.Rectangle(-1, 4, 2, 2);
			var cut = window.Cut(screen);
			cut.Should().Equal(new[]{new Desktop.Rectangle(-1, 4, 1, 2)},
			                      "because the screen cuts away the right-hand side of the window");
		}

		[Test]
		public void TestCutIntersectRight()
		{
			var screen = new Desktop.Rectangle(10, 10, 10, 10);
			var window = new Desktop.Rectangle(5, 11, 10, 4);
			var cut = window.Cut(screen);
			cut.Should().Equal(new[]{new Desktop.Rectangle(5, 11, 5, 4)},
			                   "because the screen cuts away the right-hand side of the window");
		}

		[Test]
		public void TestCutWindowBiggerThanScreen()
		{
			var screen = new Desktop.Rectangle(0, 0, 20, 10);
			var window = new Desktop.Rectangle(-10, -10, 60, 30);
			var cut = window.Cut(screen);
			cut.Should().BeEquivalentTo(new[]
			                            {
											// Top "row"
											new Desktop.Rectangle(-10, -10, 10, 10),
											new Desktop.Rectangle(0, -10, 20, 10),
											new Desktop.Rectangle(20, -10, 30, 10),

											// "Left side"
											new Desktop.Rectangle(-10, 0, 10, 10),
											// "Right side"
											new Desktop.Rectangle(20, 0, 30, 10),

											// "Bottom row"
											new Desktop.Rectangle(-10, 10, 10, 10),
											new Desktop.Rectangle(0, 10, 20, 10),
											new Desktop.Rectangle(20, 10, 30, 10)
			                            },
			                   "because the screen cuts a straight rectangle into the window, which should have been modelled as six rectangles");
		}
	}
}