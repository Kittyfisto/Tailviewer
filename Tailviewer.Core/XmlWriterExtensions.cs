using System.Xml;
using Metrolib;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Core
{
	/// <summary>
	///     Extension methods for the <see cref="XmlWriter" /> class.
	/// </summary>
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
	}
}