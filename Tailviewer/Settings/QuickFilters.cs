using System.Collections.Generic;
using System.Xml;

namespace Tailviewer.Settings
{
	internal sealed class QuickFilters
		: List<QuickFilter>
	{
		public void Restore(XmlReader reader)
		{
			var quickfilters = new List<QuickFilter>();
			var subtree = reader.ReadSubtree();
			while (subtree.Read())
			{
				switch (subtree.Name)
				{
					case "quickfilter":
						var quickfilter = new QuickFilter();
						if (quickfilter.Restore(subtree))
							quickfilters.Add(quickfilter);
						break;
				}
			}

			Clear();
			AddRange(quickfilters);
		}

		public void Save(XmlWriter writer)
		{
			foreach (var dataSource in this)
			{
				writer.WriteStartElement("quickfilter");
				dataSource.Save(writer);
				writer.WriteEndElement();
			}
		}
	}
}