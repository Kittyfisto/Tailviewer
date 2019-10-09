using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;
using System.Xml;
using log4net;
using Tailviewer.Core.Settings;

namespace Tailviewer.Settings
{
	public sealed class LogFileSettings
		: ILogFileSettings
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Implementation of ILogFileSettings

		public Encoding DefaultEncoding { get; set; }

		#endregion

		[Pure]
		public LogFileSettings Clone()
		{
			return new LogFileSettings
			{
				DefaultEncoding = DefaultEncoding
			};
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("defaultencoding", DefaultEncoding?.WebName ?? string.Empty);
		}

		public void Restore(XmlReader reader)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "defaultencoding":
						var encodingName = reader.ReadContentAsString();
						if (!string.IsNullOrEmpty(encodingName))
						{
							try
							{
								DefaultEncoding = Encoding.GetEncoding(encodingName);
							}
							catch (Exception e)
							{
								Log.WarnFormat("Caught exception while trying to get encoding '{0}':\r\n{1}",
								               encodingName,
								               e);
							}
						}
						break;
				}
			}
		}
	}
}