using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using log4net;
using Metrolib;

namespace Tailviewer.Settings
{
	public sealed class DataSources
		: List<DataSource>
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public Guid SelectedItem;

		public void Restore(XmlReader reader, out bool neededPatching)
		{
			neededPatching = false;
			var dataSources = new List<DataSource>();
			XmlReader subtree = reader.ReadSubtree();
			Guid selectedItem = Guid.Empty;

			while (subtree.Read())
			{
				switch (subtree.Name)
				{
					case "datasources": // "this"
						for (int i = 0; i < subtree.AttributeCount; ++i)
						{
							subtree.MoveToAttribute(i);
							switch (subtree.Name)
							{
								case "selecteditem":
									selectedItem = subtree.ReadContentAsGuid();
									break;
							}
						}
						break;

					case "datasource":
						var dataSource = new DataSource();
						bool sourceNeedsPatching;
						dataSource.Restore(subtree, out sourceNeedsPatching);
						dataSources.Add(dataSource);
						neededPatching |= sourceNeedsPatching;
						break;
				}
			}

			Clear();
			Capacity = dataSources.Count;
			foreach (DataSource source in dataSources)
			{
				Add(source);

				if (source.Id == selectedItem)
				{
					SelectedItem = selectedItem;
				}
			}

			if (SelectedItem != selectedItem || selectedItem == Guid.Empty)
			{
				Log.WarnFormat("Selected item '{0}' not found in data-sources, ignoring it...", selectedItem);
				if (Count > 0)
				{
					SelectedItem = this[0].Id;
				}
			}
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeGuid("selecteditem", SelectedItem);

			foreach (DataSource dataSource in this)
			{
				writer.WriteStartElement("datasource");
				dataSource.Save(writer);
				writer.WriteEndElement();
			}
		}
	}
}