using System;
using System.Reflection;
using System.Text;
using System.Xml;
using log4net;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Settings.CustomFormats
{
	public sealed class CustomLogFileFormat
		: ICustomLogFileFormat
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private PluginId _pluginId;
		private string _name;
		private string _format;
		private Encoding _encoding;

		public CustomLogFileFormat()
		{}

		public CustomLogFileFormat(PluginId pluginId, string name, string format, Encoding encoding)
		{
			_pluginId = pluginId;
			_name = name;
			_format = format;
			_encoding = encoding;
		}

		/// <summary>
		///     The id of the plugin which is able to interpret this <see cref="Format"/>.
		/// </summary>
		public PluginId PluginId => _pluginId;

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("plugin", PluginId.ToString());
			writer.WriteAttributeString("name", Name);
			writer.WriteAttributeString("format", Format);
			writer.WriteAttributeString("encoding", Encoding?.WebName ?? "");
		}

		#region Equality members

		private bool Equals(CustomLogFileFormat other)
		{
			return Equals(PluginId, other.PluginId) && string.Equals(Name, other.Name) && string.Equals(Format, other.Format) && Equals(Encoding, other.Encoding);
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is CustomLogFileFormat other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (PluginId != null ? PluginId.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Format != null ? Format.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Encoding != null ? Encoding.GetHashCode() : 0);
				return hashCode;
			}
		}

		public static bool operator ==(CustomLogFileFormat left, CustomLogFileFormat right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(CustomLogFileFormat left, CustomLogFileFormat right)
		{
			return !Equals(left, right);
		}

		#endregion

		public void Restore(XmlReader reader)
		{
			var count = reader.AttributeCount;
			for (var i = 0; i < count; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "plugin":
						_pluginId = new PluginId(reader.ReadContentAsString());
						break;

					case "name":
						_name = reader.ReadContentAsString();
						break;

					case "format":
						_format = reader.ReadContentAsString();
						break;

					case "encoding":
						var encodingName = reader.ReadContentAsString();
						if (!string.IsNullOrEmpty(encodingName))
							try
							{
								_encoding = Encoding.GetEncoding(encodingName);
							}
							catch (Exception e)
							{
								Log.WarnFormat("Caught exception while trying to get encoding '{0}':\r\n{1}",
								               encodingName,
								               e);
							}

						break;
				}
			}
		}

		#region Implementation of ICustomLogFileFormat

		public string Name => _name;

		public string Format => _format;

		public Encoding Encoding => _encoding;

		#endregion
	}
}