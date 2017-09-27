using System.Xml;
using Metrolib;

namespace Tailviewer.Core
{
	/// <summary>
	///     Extension methods for the <see cref="XmlReader" /> class.
	/// </summary>
	public static class XmlReaderExtensions
	{
		public static DataSourceId ReadContentAsDataSourceId(this XmlReader reader)
		{
			var guid = reader.ReadContentAsGuid();
			return new DataSourceId(guid);
		}

		public static QuickFilterId ReadContentAsQuickFilterId(this XmlReader reader)
		{
			var guid = reader.ReadContentAsGuid();
			return new QuickFilterId(guid);
		}

		public static AnalysisId ReadContentAsAnalysisId(this XmlReader reader)
		{
			var guid = reader.ReadContentAsGuid();
			return new AnalysisId(guid);
		}

		public static LogAnalyserId ReadContentAsLogAnalyserId(this XmlReader reader)
		{
			var guid = reader.ReadContentAsGuid();
			return new LogAnalyserId(guid);
		}

		public static WidgetId ReadContentAsWidgetId(this XmlReader reader)
		{
			var guid = reader.ReadContentAsGuid();
			return new WidgetId(guid);
		}
	}
}