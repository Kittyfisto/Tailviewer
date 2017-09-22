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

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, null))
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			var other = obj as QuickFilters;
			if (ReferenceEquals(other, null))
				return false;

			if (Count != other.Count)
				return false;

			for (int i = 0; i < Count; ++i)
			{
			}

			return true;
		}

		public override int GetHashCode()
		{
			return 42;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public bool IsEquivalent(QuickFilters other)
		{
			if (ReferenceEquals(other, null))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			if (Count != other.Count)
				return false;

			for (int i = 0; i < Count; ++i)
			{
				var filter = this[i];
				var otherFilter = other[i];

				if (!IsEquivalent(filter, otherFilter))
					return false;
			}

			return true;
		}

		private static bool IsEquivalent(QuickFilter lhs, QuickFilter rhs)
		{
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (ReferenceEquals(lhs, null))
				return false;

			return lhs.IsEquivalent(rhs);
		}
	}
}