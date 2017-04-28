using System.Collections.Generic;
using System.Xml;
using Tailviewer.Settings.Dashboard.Widgets;

namespace Tailviewer.Settings.Dashboard
{
	public sealed class LayoutSettings
		: List<WidgetSettings>
	{
		public void Restore(XmlReader reader)
		{
			var widgets = new List<WidgetSettings>();
			XmlReader subtree = reader.ReadSubtree();

			while (subtree.Read())
			{
				switch (subtree.Name)
				{
					case "layout": // "this"
						for (int i = 0; i < subtree.AttributeCount; ++i)
						{
							subtree.MoveToAttribute(i);
							switch (subtree.Name)
							{
							}
						}
						break;

					case "widget":
						var widget = new WidgetSettings();
						widget.Restore(reader);
						widgets.Add(widget);
						break;
				}
			}

			Clear();
			Capacity = widgets.Count;
			foreach (WidgetSettings widget in widgets)
			{
				Add(widget);
			}
		}

		public void Save(XmlWriter writer)
		{
			//writer.WriteAttributeGuid("selecteditem", SelectedItem);

			foreach (WidgetSettings widget in this)
			{
				writer.WriteStartElement("widget");
				widget.Save(writer);
				writer.WriteEndElement();
			}
		}
	}
}