using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Xml;
using Metrolib;

namespace Tailviewer.Settings
{
	public sealed class MainWindowSettings
		: ICloneable
	{
		public string SelectedSidePanel;

		public MainWindowSettings()
		{
			Window = new WindowSettings();
		}

		private MainWindowSettings(MainWindowSettings other)
		{
			Window = other.Window.Clone();
			SelectedSidePanel = other.SelectedSidePanel;
		}

		public WindowSettings Window { get; }

		object ICloneable.Clone()
		{
			return Clone();
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("selectedsidepanel", SelectedSidePanel);
			Window.Save(writer);
		}

		public void Restore(XmlReader reader)
		{
			for (var i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "selectedsidepanel":
						SelectedSidePanel = reader.ReadContentAsString();
						break;
				}
			}

			Window.Restore(reader);
		}

		public void UpdateFrom(Window window)
		{
			Window.UpdateFrom(window);
		}

		public void RestoreTo(Window window)
		{
			Window.RestoreTo(window);
		}

		[Pure]
		public MainWindowSettings Clone()
		{
			return new MainWindowSettings(this);
		}
	}
}