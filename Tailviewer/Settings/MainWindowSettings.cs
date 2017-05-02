using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Xml;
using Metrolib;

namespace Tailviewer.Settings
{
	public sealed class MainWindowSettings
		: IMainWindowSettings
		, ICloneable
	{
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
			set { _window.State = value; }
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

		public MainWindowSettings()
		{
			_window = new WindowSettings();
		}

		private MainWindowSettings(MainWindowSettings other)
		{
			_window = other._window.Clone();
			SelectedSidePanel = other.SelectedSidePanel;
			AlwaysOnTop = other.AlwaysOnTop;
		}

		private readonly WindowSettings _window;

		object ICloneable.Clone()
		{
			return Clone();
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("selectedsidepanel", SelectedSidePanel);
			writer.WriteAttributeBool("alwaysontop", AlwaysOnTop);
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

					case "selectedsidepanel":
						SelectedSidePanel = reader.ReadContentAsString();
						break;
				}
			}

			_window.Restore(reader);
		}

		public void UpdateFrom(Window window)
		{
			_window.UpdateFrom(window);
		}

		public void RestoreTo(Window window)
		{
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