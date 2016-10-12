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

		public static DateTime ReadContentAsDateTime2(this XmlReader reader)
		{
			string stringValue = reader.Value;
			long value = long.Parse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
			var dateTime = new DateTime(value);
			return dateTime;
		}

		public static double ReadContentAsDouble2(this XmlReader reader)
		{
			string stringValue = reader.Value;
			double value = double.Parse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture);
			return value;
		}

		public static bool ReadContentAsBool(this XmlReader reader)
		{
			string value = reader.Value;
			return string.Equals(value, "true", StringComparison.InvariantCultureIgnoreCase);
		}

		public static byte[] ReadContentAsBase64(this XmlReader reader)
		{
			string stringValue = reader.Value;
			var value = Convert.FromBase64String(stringValue);
			return value;
		}

		public static void WriteAttributeDateTime(this XmlWriter writer, string localName, DateTime value)
		{
			string stringValue = value.Ticks.ToString(CultureInfo.InvariantCulture);
			writer.WriteAttributeString(localName, stringValue);
		}

		public static void WriteAttributeDouble(this XmlWriter writer, string localName, double value)
		{
			string stringValue = value.ToString(CultureInfo.InvariantCulture);
			writer.WriteAttributeString(localName, stringValue);
		}

		public static void WriteAttributeGuid(this XmlWriter writer, string localName, Guid value)
		{
			string stringValue = value.ToString();
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

		public static void WriteAttributeBase64(this XmlWriter writer, string localName, byte[] value)
		{
			var stringValue = value != null
				                  ? Convert.ToBase64String(value)
				                  : string.Empty;

			writer.WriteAttributeString(localName, stringValue);
		}
	}
}