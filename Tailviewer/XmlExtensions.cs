using System;
using System.Globalization;
using System.Xml;

namespace Tailviewer
{
	public static class XmlExtensions
	{
		public static T ReadContentAsEnum<T>(this XmlReader reader)
		{
			string value = reader.Value;
			return (T) Enum.Parse(typeof (T), value);
		}

		public static Guid ReadContentAsGuid(this XmlReader reader)
		{
			string value = reader.Value;
			return Guid.Parse(value);
		}

		public static bool ReadContentAsBool(this XmlReader reader)
		{
			string value = reader.Value;
			return string.Equals(value, "true", StringComparison.InvariantCultureIgnoreCase);
		}

		public static void WriteAttributeGuid(this XmlWriter writer, string localName, Guid value)
		{
			var stringValue = value.ToString();
			writer.WriteAttributeString(localName, stringValue);
		}

		public static void WriteAttributeBool(this XmlWriter writer, string localName, bool value)
		{
			writer.WriteAttributeString(localName, value ? "true" : "false");
		}

		public static void WriteAttributeInt(this XmlWriter writer, string localname, int value)
		{
			writer.WriteAttributeString(localname, value.ToString(CultureInfo.InvariantCulture));
		}

		public static void WriteAttributeEnum<T>(this XmlWriter writer, string localName, T value)
		{
			writer.WriteAttributeString(localName, value.ToString());
		}
	}
}