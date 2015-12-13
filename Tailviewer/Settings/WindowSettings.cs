using System;
using System.Globalization;
using System.Windows;
using System.Xml;
using Tailviewer.Ui.Controls;

namespace Tailviewer.Settings
{
	/// <summary>
	///     Represents the configuration of a window.
	///     Currently only preserves a window's position + dimension, but may be expanded in the future.
	/// </summary>
	public sealed class WindowSettings
	{
		public double Height;
		public double Left;
		public WindowState State;
		public double Top;
		public double Width;

		public void RestoreTo(Window window)
		{
			window.Left = Left;
			window.Top = Top;
			window.Width = Width;
			window.Height = Height;
			window.WindowState = State;
		}

		public void Restore(XmlReader reader)
		{
			int count = reader.AttributeCount;
			for (int i = 0; i < count; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "top":
						Top = reader.ReadContentAsDouble();
						break;

					case "left":
						Left = reader.ReadContentAsDouble();
						break;

					case "width":
						Width = reader.ReadContentAsDouble();
						break;

					case "height":
						Height = reader.ReadContentAsDouble();
						break;

					case "state":
						State = (WindowState) Enum.Parse(typeof(WindowState), reader.Value);
						break;
				}
			}
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("top", Top.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("left", Left.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("width", Width.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("height", Height.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("state", State.ToString());
		}

		public void UpdateFrom(MainWindow window)
		{
			Left = window.Left;
			Top = window.Top;
			Width = window.Width;
			Height = window.Height;
			State = window.WindowState;
		}
	}
}