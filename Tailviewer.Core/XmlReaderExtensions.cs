using System.Xml;
using Metrolib;

namespace Tailviewer.Core
{
	/// <summary>
	///     Extension methods for the <see cref="XmlReader" /> class.
	/// </summary>
	public static class XmlReaderExtensions
	{
		/// <summary>
		///     Reads the contents as a <see cref="DataSourceId" />.
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static DataSourceId ReadContentAsDataSourceId(this XmlReader reader)
		{
			var guid = reader.ReadContentAsGuid();
			return new DataSourceId(guid);
		}

		/// <summary>
		///     Reads the contents as a <see cref="QuickFilterId" />.
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static QuickFilterId ReadContentAsQuickFilterId(this XmlReader reader)
		{
			var guid = reader.ReadContentAsGuid();
			return new QuickFilterId(guid);
		}
	}
}