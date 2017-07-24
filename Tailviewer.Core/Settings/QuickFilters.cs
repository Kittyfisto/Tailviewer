using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Tailviewer.Core.Settings
{
	public sealed class QuickFilters
		: List<QuickFilter>
		, ICloneable
	{
		public void Restore(XmlReader reader)
		{
			var quickfilters = new List<QuickFilter>();
			XmlReader subtree = reader.ReadSubtree();
			while (subtree.Read())
			{
				switch (subtree.Name)
				{
					case "quickfilter":
						var quickfilter = new QuickFilter();
						if (quickfilter.Restore(subtree))
						{
							quickfilters.Add(quickfilter);
						}
						break;
				}
			}

			Clear();
			AddRange(quickfilters);
		}

		public void Save(XmlWriter writer)
		{
			foreach (QuickFilter dataSource in this)
			{
				writer.WriteStartElement("quickfilter");
				dataSource.Save(writer);
				writer.WriteEndElement();
			}
		}

		public QuickFilters Clone()
		{
			var filters = new QuickFilters();
			filters.AddRange(this.Select(x => x.Clone()));
			return filters;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}