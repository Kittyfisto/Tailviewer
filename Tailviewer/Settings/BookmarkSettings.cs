using System;
using System.Xml;
using Metrolib;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Settings
{
	public sealed class BookmarkSettings
	{
		public Guid DataSourceId;
		public LogLineIndex Index;

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeGuid("datasource", DataSourceId);
			writer.WriteAttributeInt("index", Index.Value);
		}

		public void Restore(XmlReader reader)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "datasource":
						DataSourceId = reader.ReadContentAsGuid();
						break;

					case "index":
						Index = reader.ReadContentAsInt();
						break;
				}
			}
		}
	}
}