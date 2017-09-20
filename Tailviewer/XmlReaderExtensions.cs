using System.Xml;
using Metrolib;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer
{
	public static class XmlReaderExtensions
	{
		public static DataSourceId ReadContentAsDataSourceId(this XmlReader reader)
		{
			var guid = reader.ReadContentAsGuid();
			return new DataSourceId(guid);
		}
	}
}