using System;
using System.Reflection;
using System.Text;
using System.Xml;
using log4net;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Settings.CustomFormats
{
	public sealed class CustomLogFileFormat
		: ICustomLogFileFormat
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		///     The id of the plugin which is able to interpret this <see cref="Format"/>.
		/// </summary>
		public PluginId PluginId { get; set; }

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("plugin", PluginId.ToString());
			writer.WriteAttributeString("name", Name);
			writer.WriteAttributeString("format", Format);
			writer.WriteAttributeString("encoding", Encoding?.WebName ?? "");
		}

		public void Restore(XmlReader reader)
		{
			var count = reader.AttributeCount;
			for (var i = 0; i < count; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "plugin":
						PluginId = new PluginId(reader.ReadContentAsString());
						break;

					case "name":
						Name = reader.ReadContentAsString();
						break;

					case "format":
						Format = reader.ReadContentAsString();
						break;

					case "encoding":
						var encodingName = reader.ReadContentAsString();
						if (!string.IsNullOrEmpty(encodingName))
							try
							{
								Encoding = Encoding.GetEncoding(encodingName);
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

		public string Name { get; set; }

		public string Format { get; set; }

		public Encoding Encoding { get; set; }

		#endregion
	}
}