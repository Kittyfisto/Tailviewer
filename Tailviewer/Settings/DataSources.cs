using System.Collections.Generic;
using System.Xml;

namespace Tailviewer.Settings
{
	internal sealed class DataSources
		: List<DataSource>
	{
		public void Restore(XmlReader reader, out bool neededPatching)
		{
			neededPatching = false;
			var dataSources = new List<DataSource>();
			var subtree = reader.ReadSubtree();
			while (subtree.Read())
			{
				switch (subtree.Name)
				{
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