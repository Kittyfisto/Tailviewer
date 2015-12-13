using System.Collections.Generic;
using System.Xml;

namespace Tailviewer.Settings
{
	internal sealed class DataSourcesSettings
		: List<DataSourceSettings>
	{
		public void Restore(XmlReader reader)
		{
			var dataSources = new List<DataSourceSettings>();
			var subtree = reader.ReadSubtree();
			while (subtree.Read())
			{
				switch (subtree.Name)
				{
					case "datasource":
						var dataSource = new DataSourceSettings();
						dataSource.Restore(subtree);
						dataSources.Add(dataSource);
						break;
				}
			}

			Clear();
			AddRange(dataSources);
		}

		public void Save(XmlWriter writer)
		{
			foreach (var dataSource in this)
			{
				writer.WriteStartElement("datasource");
				dataSource.Save(writer);
				writer.WriteEndElement();
			}
		}
	}
}