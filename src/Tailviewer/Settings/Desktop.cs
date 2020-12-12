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
		/// <param name="settings"></param>
		/// <returns>
		///     A new object with corrected position and / or area or the existing object in case no modification was
		///     necessary.
		/// </returns>
		[Pure]
		public WindowSettings ClipToBoundaries(WindowSettings settings)
		{
			// If we didn't detect any monitors then somethings going on, but whatever it may be, we cannot move on...
			if (_screens.Count == 0)
			{
				Log.WarnFormat("Unable to fit window into current desktop area: Not a single monitor was detected!");
				return settings;
			}

			// We'll first check if the window fits into one existing screen already.
			// If it does, then we don't need to do anything.
			foreach (var screen in _screens)
				if (screen.Contains(settings))
					return settings;

			Log.InfoFormat("The window doesn't fit into the current desktop area anymore, trying to find a more suitable location...");

			// We'll try to place the window on the closest screen to its current position.
			// This is incredibly useful if the window is only partially hidden.
			var screensSortedByDistance = SortScreensByDistanceToWindow(settings, _screens);
			foreach (var screen in screensSortedByDistance)
			{
				if (screen.TryMoveWindowIntoScreen(settings, out var newSettings))
				{
					Log.InfoFormat("Found new window location: {0}", newSettings);
					return newSettings;
				}
			}

			Log.InfoFormat("The window's size doesn't fit into any of the available monitor(s): Resizing the window so it will fit");

			// Moving the window alone doesn't cut it, we'll have to reduce the windows size to accomodate
			// the apparent change in screens.
			// We'll ditch our closest screen approach and instead try to move the window to the screen with
			// the greatest screen area.
			var screensSortedByArea = SortScreensByArea(_screens);
			var modifiedSettings = screensSortedByArea[0].FitWindowInto(settings);
			Log.InfoFormat("Found new window location and size: {0}", modifiedSettings);
			return modifiedSettings;
		}

		[Pure]
		private static IReadOnlyCollection<Screen> SortScreensByDistanceToWindow(
			WindowSettings window,
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

		/// <summary>
		///     Represents one screen connected to the current system.
		/// </summary>
		public sealed class Screen
		{
			private readonly Size _size;
			private readonly Point _topLeft;

			public Screen(Point topLeft, Size size)
			{
				_topLeft = topLeft;
				_size = size;
			}

			public Screen(System.Windows.Forms.Screen screen)
				: this(new Point(screen.Bounds.X, screen.Bounds.Y),
				       new Size(screen.Bounds.Width, screen.Bounds.Height))
			{
			}

			public Point TopLeft
			{
				get { return _topLeft; }
			}

			public Point TopRight
			{
				get { return new Point(_topLeft.X + _size.Width, _topLeft.Y); }
			}

			public Point BottomRight
			{
				get { return new Point(_topLeft.X + _size.Width, _topLeft.Y + _size.Height); }
			}

			public Point BottomLeft
			{
				get { return new Point(_topLeft.X, _topLeft.Y + _size.Height); }
			}

			public Point Center
			{
				get { return new Point(_topLeft.X + _size.Width / 2, _topLeft.Y + _size.Height / 2); }
			}

			public double Area
			{
				get { return _size.Width * _size.Height; }
			}

			/// <summary>
			///     Tests if this screen fully contains the given window.
			///     If the window is only partially contained (e.g. is bigger than this screen) or if
			///     the window is completely outside this screen, then false is returned.
			/// </summary>
			/// <param name="settings"></param>
			/// <returns></returns>
			[Pure]
			public bool Contains(WindowSettings settings)
			{
				return !(settings.Left < _topLeft.X ||
				         settings.Top < _topLeft.Y ||
				         settings.Left + settings.Width > TopRight.X ||
				         settings.Top + settings.Height > BottomRight.Y);
			}

			/// <summary>
			///    Tries to bring the given window into view.
			/// </summary>
			/// <param name="settings"></param>
			/// <param name="movedSettings"></param>
			/// <returns></returns>
			public bool TryMoveWindowIntoScreen(WindowSettings settings, out WindowSettings movedSettings)
			{
				movedSettings = null;

				if (settings.Width > _size.Width)
					return false;
				if (settings.Height > _size.Height)
					return false;

				// The window will fit, we'll just have to move it into view.
				// Let's place it so its centered on the screen...
				var halfWidth = settings.Width / 2;
				var halfHeight = settings.Height / 2;
				var center = Center;
				var topLeft = new Point(center.X - halfWidth, center.Y - halfHeight);
				movedSettings = settings.Clone();
				movedSettings.Left = topLeft.X;
				movedSettings.Top = topLeft.Y;
				return true;
			}

			[Pure]
			public WindowSettings FitWindowInto(WindowSettings settings)
			{
				var newSettings = settings.Clone();
				newSettings.Left = _topLeft.X;
				newSettings.Top = _topLeft.Y;
				newSettings.Width = _size.Width;
				newSettings.Height = _size.Height;
				return newSettings;
			}
		}
	}
}