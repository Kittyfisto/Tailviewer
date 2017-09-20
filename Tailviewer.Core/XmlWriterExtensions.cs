using System.Xml;
using Metrolib;

namespace Tailviewer.Core
{
	public static class XmlWriterExtensions
	{
		public static void WriteAttribute(this XmlWriter writer, string localName, DataSourceId id)
		{
			writer.WriteAttributeGuid(localName, id.Value);
		}

		public static void WriteAttribute(this XmlWriter writer, string localName, QuickFilterId id)
		{
			writer.WriteAttributeGuid(localName, id.Value);
		}

		public static void WriteAttribute(this XmlWriter writer, string localName, AnalysisId id)
		{
			writer.WriteAttributeGuid(localName, id.Value);
		}

		public static void WriteAttribute(this XmlWriter writer, string localName, LogAnalyserId id)
		{
			writer.WriteAttributeGuid(localName, id.Value);
		}

		public static void WriteAttribute(this XmlWriter writer, string localName, WidgetId id)
		{
			writer.WriteAttributeGuid(localName, id.Value);
		}
	}
}
