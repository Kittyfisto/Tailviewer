using System;
using System.Globalization;
using System.Windows.Media;
using System.Xml;
using log4net;
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="attributeName"></param>
		/// <param name="log"></param>
		/// <param name="defaultValue"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static void ReadAttributeAsInt(this XmlReader reader, string attributeName, ILog log, int defaultValue, out int value)
		{
			if (!TryMoveToAttribute(reader, attributeName, log))
			{
				value = defaultValue;
			}
			else
			{
				var content = reader.ReadContentAsString();
				if (!int.TryParse(content, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
				{
					log.WarnFormat("Cannot parse value '{0}' as an integer, restoring default value for attribute '{1}' instead",
					               content,
					               attributeName);
					value = defaultValue;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="attributeName"></param>
		/// <param name="log"></param>
		/// <param name="defaultValue"></param>
		/// <param name="value"></param>
		public static void ReadAttributeAsColor(this XmlReader reader, string attributeName, ILog log, Color defaultValue, out Color value)
		{
			if (!TryMoveToAttribute(reader, attributeName, log))
			{
				value = defaultValue;
			}
			else
			{
				var content = reader.ReadContentAsString();
				try
				{
					value = (Color) ColorConverter.ConvertFromString(content ?? string.Empty);
				}
				catch (Exception e)
				{
					log.WarnFormat("Cannot parse value '{0}' as an integer, restoring default value for attribute '{1}' instead:\r\n{2}",
					               content,
					               attributeName,
					               e);
					value = defaultValue;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="attributeName"></param>
		/// <param name="log"></param>
		/// <returns></returns>
		private static bool TryMoveToAttribute(this XmlReader reader, string attributeName, ILog log)
		{
			if (!reader.MoveToAttribute(attributeName))
			{
				log.InfoFormat("Cannot find attribute '{0}', restoring default value instead", attributeName);
				return false;
			}

			return true;
		}
	}
}