using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Windows;
using System.Xml;
using log4net;
using Metrolib;

namespace Tailviewer.Settings
{
	public sealed class MainWindowSettings
		: IMainWindowSettings
		, ICloneable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private WindowState _previousWindowState = WindowState.Normal;

		public double Height
		{
			get { return _window.Height; }
			set { _window.Height = value; }
		}

		public double Left
		{
			get { return _window.Left; }
			set { _window.Left = value; }
		}

		public WindowState State
		{
			get { return _window.State; }
			set
			{
				if (value == _window.State)
					return;

				_previousWindowState = _window.State;
				_window.State = value;
			}
		}

		public double Top
		{
			get { return _window.Top; }
			set { _window.Top = value; }
		}

		public double Width
		{
			get { return _window.Width; }
			set { _window.Width = value; }
		}

		public bool AlwaysOnTop { get; set; }

		public string SelectedSidePanel { get; set; }

		public bool IsLeftSidePanelVisible { get; set; }

		public string SelectedMainPanel { get; set; }

		public MainWindowSettings()
		{
			_window = new WindowSettings();
			IsLeftSidePanelVisible = true;
		}

		private MainWindowSettings(MainWindowSettings other)
		{
			_window = other._window.Clone();
			SelectedMainPanel = other.SelectedMainPanel;
			SelectedSidePanel = other.SelectedSidePanel;
			AlwaysOnTop = other.AlwaysOnTop;
			IsLeftSidePanelVisible = other.IsLeftSidePanelVisible;
		}

		private WindowSettings _window;

		object ICloneable.Clone()
		{
			return Clone();
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("selectedmainpanel", SelectedMainPanel);
			writer.WriteAttributeString("selectedsidepanel", SelectedSidePanel);
			writer.WriteAttributeBool("alwaysontop", AlwaysOnTop);
			writer.WriteAttributeBool("isleftsidepanelvisible", IsLeftSidePanelVisible);
			writer.WriteAttributeEnum("previouswindowstate", _previousWindowState);
			_window.Save(writer);
		}

		public void Restore(XmlReader reader)
		{
			for (var i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "alwaysontop":
						AlwaysOnTop = reader.ReadContentAsBool();
						break;

					case "isleftsidepanelvisible":
						IsLeftSidePanelVisible = reader.ReadContentAsBool();
						break;

					case "selectedmainpanel":
						SelectedMainPanel = reader.ReadContentAsString();
						break;

					case "selectedsidepanel":
						SelectedSidePanel = reader.ReadContentAsString();
						break;

					case "previouswindowstate":
						_previousWindowState = reader.ReadContentAsEnum<WindowState>();
						break;
				}
			}

			_window.Restore(reader);
		}

		public void UpdateFrom(Window window)
		{
			_window.UpdateFrom(window);

			// For some reason, when the window is minimized, the width and height
			// are set to small values such as 160/28 whereas the ActualWidth and ActualHeight
			// values are sensible. Don't know why that is, but we want to store the sensible values!
			if (window.Width < window.ActualWidth)
				_window.Width = window.ActualWidth;
			if (window.Height < window.ActualHeight)
				_window.Height = window.ActualHeight;

			Log.DebugFormat("Updated main window settings to : Width={0} Height={1}", Width, Height);
		}

		public void ClipToBounds(Desktop desktop)
		{
			try
			{
				var currentRectangle = new Desktop.Window(_window);
				var newRectangle = desktop.ClipToBoundaries(currentRectangle);
				if (newRectangle != currentRectangle)
				{
					_window.Left = newRectangle.Left;
					_window.Top = newRectangle.Top;
					_window.Width = newRectangle.Width;
					_window.Height = newRectangle.Height;
					if (newRectangle.IsMaximized)
						_window.State = WindowState.Maximized;
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while trying to clip window to desktop bounds: {0}", e);
			}
		}

		public void RestoreTo(Window window)
		{
			// https://github.com/Kittyfisto/Tailviewer/issues/250
			// When we start the application, a user NEVER wants the app
			// to start-up minimized. Therefore we start with the previous state
			// just in case the app was minimized before it was shut-down.
			if (_window.State == WindowState.Minimized)
				_window.State = _previousWindowState;

			_window.RestoreTo(window);
			window.Topmost = AlwaysOnTop;
		}

		[Pure]
		public MainWindowSettings Clone()
		{
			return new MainWindowSettings(this);
		}
	}
}