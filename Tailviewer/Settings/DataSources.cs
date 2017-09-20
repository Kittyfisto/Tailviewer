using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using log4net;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Settings
{
	public sealed class DataSources
		: List<DataSource>
		, IDataSourcesSettings
		, ICloneable
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public DataSourceId SelectedItem { get; set; }

		object ICloneable.Clone()
		{
			return Clone();
		}

		public void Restore(XmlReader reader, out bool neededPatching)
		{
			neededPatching = false;
			var dataSources = new List<DataSource>();
			var subtree = reader.ReadSubtree();
			var selectedItem = DataSourceId.Empty;

			while (subtree.Read())
				switch (subtree.Name)
				{
					case "datasources": // "this"
						for (var i = 0; i < subtree.AttributeCount; ++i)
						{
							subtree.MoveToAttribute(i);
							switch (subtree.Name)
							{
								case "selecteditem":
									selectedItem = subtree.ReadContentAsDataSourceId();
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

			Clear();
			Capacity = dataSources.Count;
			foreach (var source in dataSources)
			{
				Add(source);

				if (source.Id == selectedItem)
					SelectedItem = selectedItem;
			}

			if (SelectedItem != selectedItem || selectedItem == DataSourceId.Empty)
			{
				Log.WarnFormat("Selected item '{0}' not found in data-sources, ignoring it...", selectedItem);
				if (Count > 0)
					SelectedItem = this[0].Id;
			}
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttribute("selecteditem", SelectedItem);

			foreach (var dataSource in this)
			{
				writer.WriteStartElement("datasource");
				dataSource.Save(writer);
				writer.WriteEndElement();
			}
		}

		public DataSources Clone()
		{
			var clone = new DataSources();
			clone.AddRange(this.Select(x => x.Clone()));
			clone.SelectedItem = SelectedItem;
			return clone;
		}

		/// <summary>
		///     Moves the element <paramref name="dataSource" /> to appear *anchor*
		///     <paramref name="anchor" />. Does nothing if this constraint doesn't
		///     exist of if either are not part of this list.
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="anchor"></param>
		public void MoveBefore(DataSource dataSource, DataSource anchor)
		{
			int dataSourceIndex = IndexOf(dataSource);
			int anchorIndex = IndexOf(anchor);
			if (dataSourceIndex != -1 && anchorIndex != -1)
			{
				// The required constraint already is true: dataSource is before 'anchor'
				if (dataSourceIndex < anchorIndex)
					return;

				RemoveAt(dataSourceIndex);
				Insert(anchorIndex, dataSource);
			}
		}
	}
}