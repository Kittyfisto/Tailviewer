using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using log4net;
using Metrolib;
using Size = System.Windows.Size;

namespace Tailviewer.Settings
{
	/// <summary>
	/// </summary>
	/// <remarks>
	///     TODO: This might be of interest for others and could be moved to Metrolib.
	/// </remarks>
	public sealed class Desktop
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IReadOnlyCollection<Screen> _screens;

		public static Desktop Current
		{
			get
			{
				return new Desktop(System.Windows.Forms.Screen.AllScreens.Select(x => new Screen(x)).ToList());
			}
		}

		public Desktop(IReadOnlyCollection<Screen> screens)
		{
			_screens = screens;
		}

		/// <summary>
		///     Clips the given settings so the window fits into the current desktop.
		/// </summary>
		/// <remarks>
		///     The current implementation tries to **first** move the window to an available
		///     screen and only starts to reduce the width / height of the window if either monitor
		///     has a lower resolution than the current width / height requires.
		/// </remarks>
		/// <param name="window"></param>
		/// <returns>
		///     A new object with corrected position and / or area or the existing object in case no modification was
		///     necessary.
		/// </returns>
		[Pure]
		public Rectangle ClipToBoundaries(Rectangle window)
		{
			// If we didn't detect any monitors then somethings going on, but whatever it may be, we cannot move on...
			if (_screens.Count == 0)
			{
				Log.WarnFormat("Unable to fit window into current desktop area: Not a single monitor was detected!");
				return window;
			}

			// We'll first check if the window fits into the current virtual desktop area.
			// If it consists of more than one screen, then we'll try to figure out if there's a portion
			// of the window that's hidden, and if there is, only then will we try to move / resize the
			// window to become visible again.
			if (Contains(window))
				return window;

			Log.InfoFormat("The window doesn't fit into the current desktop area anymore, trying to find a more suitable location...");

			// We'll try to place the window on the closest screen to its current position.
			// This is incredibly useful if the window is only partially hidden.
			var screensSortedByDistance = SortScreensByDistanceToWindow(window, _screens);
			foreach (var screen in screensSortedByDistance)
			{
				if (screen.TryMoveWindowIntoScreen(window, out var newWindow))
				{
					Log.InfoFormat("Found new window location: {0}", newWindow);
					return newWindow;
				}
			}

			Log.InfoFormat("The window's size doesn't fit into any of the available monitor(s): Resizing the window so it will fit");

			// Moving the window alone doesn't cut it, we'll have to reduce the windows size to accomodate
			// the apparent change in screens.
			// We'll ditch our closest screen approach and instead try to move the window to the screen with
			// the greatest screen area.
			var screensSortedByArea = SortScreensByArea(_screens);
			var modifiedSettings = screensSortedByArea[0].FitWindowInto(window);
			Log.InfoFormat("Found new window location and size: {0}", modifiedSettings);
			return modifiedSettings;
		}

		[Pure]
		public bool Contains(Rectangle window)
		{
			// We'll cut away from the given window until all screens have performed a cut.
			IReadOnlyList<Rectangle> windowPortions = new List<Rectangle> {window};
			foreach (var screen in _screens)
			{
				windowPortions = screen.CutAway(windowPortions);
			}

			// If the screens cut away every single portion of the window then we can conclude
			// that everything is showing up as expected (even if the window is placed
			// among multiple screen boundaries).
			return windowPortions.Count == 0;
		}

		[Pure]
		private static IReadOnlyCollection<Screen> SortScreensByDistanceToWindow(
			Rectangle window,
			IReadOnlyCollection<Screen> screens)
		{
			var centerPoint = new Point(window.Left + window.Width / 2, window.Top + window.Height / 2);
			var sorted = screens.OrderBy(x => x.Center.SquaredDistanceTo(centerPoint)).ToList();
			return sorted;
		}

		[Pure]
		private static IReadOnlyList<Screen> SortScreensByArea(IReadOnlyCollection<Screen> screens)
		{
			var sorted = screens.OrderBy(x => x.Area).ToList();
			return sorted;
		}

		public struct Point
		{
			public readonly double X;
			public readonly double Y;

			public Point(double x, double y)
			{
				X = x;
				Y = y;
			}

			[Pure]
			public double DistanceTo(Point other)
			{
				return Math.Sqrt(SquaredDistanceTo(other));
			}

			[Pure]
			public double SquaredDistanceTo(Point other)
			{
				var dX = X - other.X;
				var dY = Y - other.Y;
				return dX * dX + dY * dY;
			}

			#region Overrides of ValueType

			public override string ToString()
			{
				return $"{X}, {Y}";
			}

			#endregion
		}

		public class Rectangle
		{
			#region Equality members

			protected bool Equals(Rectangle other)
			{
				return _size.Equals(other._size) && _topLeft.Equals(other._topLeft);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((Rectangle) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (_size.GetHashCode() * 397) ^ _topLeft.GetHashCode();
				}
			}

			public static bool operator ==(Rectangle left, Rectangle right)
			{
				return Equals(left, right);
			}

			public static bool operator !=(Rectangle left, Rectangle right)
			{
				return !Equals(left, right);
			}

			#endregion

			private readonly Size _size;
			private readonly Point _topLeft;

			public Rectangle()
			{}
			public Rectangle(double left, double top, double width, double height)
				: this(new Point(left, top), new Size(width, height))
			{}

			public Rectangle(Point topLeft, Size size)
			{
				_topLeft = topLeft;
				_size = size;
			}

			public Rectangle(WindowSettings settings)
				: this(new Point(settings.Left, settings.Top), new Size(settings.Width, settings.Height))
			{ }

			public double Top
			{
				get { return _topLeft.Y; }
			}

			public double Left
			{
				get { return _topLeft.X; }
			}

			public double Bottom
			{
				get { return _topLeft.Y + _size.Height; }
			}

			public double Right
			{
				get { return _topLeft.X + _size.Width; }
			}

			public Point TopLeft
			{
				get { return _topLeft; }
			}

			public Point TopRight
			{
				get { return new Point(Right, Top); }
			}

			public Point BottomRight
			{
				get { return new Point(Right, Bottom); }
			}

			public Point BottomLeft
			{
				get { return new Point(Left, Bottom); }
			}

			public Point Center
			{
				get { return new Point(_topLeft.X + _size.Width / 2, _topLeft.Y + _size.Height / 2); }
			}

			public double Area
			{
				get { return _size.Width * _size.Height; }
			}

			public double Width
			{
				get { return _size.Width; }
			}

			public double Height
			{
				get { return _size.Height; }
			}

			public Size Size
			{
				get { return _size; }
			}

			#region Overrides of Object

			public override string ToString()
			{
				return $"(({TopLeft}) ({Size}))";
			}

			#endregion

			/// <summary>
			///     Tests if this screen fully contains the given window.
			///     If the window is only partially contained (e.g. is bigger than this screen) or if
			///     the window is completely outside this screen, then false is returned.
			/// </summary>
			/// <param name="settings"></param>
			/// <returns></returns>
			[Pure]
			public bool Contains(Rectangle settings)
			{
				return !(settings.Left < Left ||
				         settings.Top < Top ||
				         settings.Right > Right ||
				         settings.Bottom > Bottom);
			}

			/// <summary>
			/// Returns the shape (split up into zero or more rectangles) that results if one were
			/// to cut away the portion of this rectangle, which intersects with the given one.
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			[Pure]
			public IReadOnlyList<Rectangle> Cut(Rectangle other)
			{
				// If the other rectangle fully contains this one then nothing
				// remains after the cut...
				if (other.Contains(this))
					return new Rectangle[0];

				// If on the other hand, the two rectangles don't overlap even one bit,
				// then nothing changes due to the cut and we can simply return this rectangle
				if (!other.Overlap(this))
					return new Rectangle[]{this};

				/*
				// So we know that there's a partial overlap. We'll just have to figure
				// out what the resulting shape looks like.
				// There's several possibilities:
				// 1. The cut removes an inside portion of this rectangle
				if (Contains(other))
					throw new NotImplementedException("Cutting holes not implemented");
				*/

				IEnumerable<Rectangle> CutOnce(IEnumerable<Rectangle> rectangles)
				{
					var pass1 = rectangles.SelectMany(rectangle => rectangle.CutHorizontal(other.Top)).ToList();
					var pass2 = pass1.SelectMany(rectangle => rectangle.CutHorizontal(other.Bottom)).ToList();
					var pass3 = pass2.SelectMany(rectangle => rectangle.CutVertical(other.Left)).ToList();
					var pass4 = pass3.SelectMany(rectangle => rectangle.CutVertical(other.Right)).ToList();
					return pass4;
				}

				// We want to find out the area of this rectangle, which is left when the overlapping area with the other
				// rectangle is cut away. This can be achieved in the following way:
				// We first cut this rectangle into pieces along the four (infinite) lines of the other rectangle.
				// Then we do a second pass where we cut the resulting rectangles once more by the same lines again.
				// Now we should have a list of up to 9 rectangles which are either **completely** outside or
				// completely inside the screen, i.e. there's no intersections anymore.
				// All that is left to do now is to remove those rectangles which are part of the screen and we're
				// left with those hat are outside of the screen.
				// This algorithm accounts for all types of intersections:
				// - Intersection along one edge of the screen resulting in half of the window to be cut
				// - Intersection along two edges of the screen, resulting in a "corner" to be cut, yielding an L-shape
				// - Full intersection in which the screen cuts away a hole inside the window.
				//var pass1 = CutOnce(new[] {this});
				//var pass2 = CutOnce(pass1);
				var tmpResult = CutOnce(new[] {this});
				// The result is almost there, after we've cut up this rectangle, we want to remove
				// those portions that are completely inside the screen, leaving only those parts which do not.
				var finalResult = tmpResult.Where(x => !other.Contains(x)).ToList();
				return finalResult;
			}

			[Pure]
			private IEnumerable<Rectangle> CutHorizontal(double y)
			{
				if (y <= Top || y >= Bottom)
					return new[] {this};

				return new[]
				{
					new Rectangle(Left, Top, Width, y - Top),
					new Rectangle(Left, y, Width, Bottom - y),
				};
			}

			[Pure]
			private IEnumerable<Rectangle> CutVertical(double x)
			{
				if (x <= Left || x >= Right)
					return new[] {this};

				return new[]
				{
					new Rectangle(Left, Top, x - Left, Height),
					new Rectangle(x, Top, Right - x, Height)
				};
			}

			[Pure]
			public bool Overlap(Rectangle rectangle)
			{
				if (rectangle.Contains(TopLeft) || rectangle.Contains(TopRight) ||
				    rectangle.Contains(BottomLeft) || rectangle.Contains(BottomRight))
					return true;

				if (Contains(rectangle.TopLeft) || Contains(rectangle.TopRight) ||
				    Contains(rectangle.BottomLeft) || rectangle.Contains(BottomRight))
					return true;

				return false;
			}

			[Pure]
			public bool Contains(Point topLeft)
			{
				return !(topLeft.X < Left || topLeft.Y > Right ||
				       topLeft.Y < Top || topLeft.Y > Bottom);
			}
		}

		/// <summary>
		///     Represents one screen connected to the current system.
		/// </summary>
		public sealed class Screen
			: Rectangle
		{

			public Screen(Point topLeft, Size size)
				: base(topLeft, size)
			{ }

			public Screen(System.Windows.Forms.Screen screen)
				: base(new Point(screen.Bounds.X, screen.Bounds.Y),
				       new Size(screen.Bounds.Width, screen.Bounds.Height))
			{
			}

			/// <summary>
			///    Tries to bring the given window into view.
			/// </summary>
			/// <param name="window"></param>
			/// <param name="movedWindow"></param>
			/// <returns></returns>
			public bool TryMoveWindowIntoScreen(Rectangle window, out Rectangle movedWindow)
			{
				movedWindow = null;

				if (window.Width > Size.Width)
					return false;
				if (window.Height > Size.Height)
					return false;

				// The window will fit, we'll just have to move it into view.
				// Let's place it so its centered on the screen...
				var halfWidth = window.Width / 2;
				var halfHeight = window.Height / 2;
				var center = Center;
				var topLeft = new Point(center.X - halfWidth, center.Y - halfHeight);
				movedWindow = new Rectangle(topLeft, window.Size);
				return true;
			}

			[Pure]
			public Rectangle FitWindowInto(Rectangle settings)
			{
				var newSettings = new Rectangle(TopLeft, Size);
				return newSettings;
			}

			[Pure]
			public IReadOnlyList<Rectangle> CutAway(IReadOnlyList<Rectangle> windowPortions)
			{
				var result = new List<Rectangle>();
				foreach (var window in windowPortions)
				{
					result.AddRange(window.Cut(this));
				}

				return result;
			}
		}
	}
}