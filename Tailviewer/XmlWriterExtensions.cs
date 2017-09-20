using System.Xml;
using Metrolib;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer
{
	public static class XmlWriterExtensions
	{
		public static void WriteAttribute(this XmlWriter writer, string localName, DataSourceId id)
		{
			writer.WriteAttributeGuid(localName, id.Value);
		}
	}
}
