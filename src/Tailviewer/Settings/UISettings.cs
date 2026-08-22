using System.Diagnostics.Contracts;
using System.Reflection;
using System.Windows.Media;
using System.Xml;
using log4net;
using Tailviewer.Core;

namespace Tailviewer.Settings
{
	public sealed class UISettings
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public const string DefaultLanguage = "en";

		public static readonly Color DefaultThemeColor = Color.FromRgb(0x00, 0x47, 0xAB);

		public string Language { get; set; }

		public Color ThemeColor { get; set; }

		public UISettings()
		{
			Language = DefaultLanguage;
			ThemeColor = DefaultThemeColor;
		}

		[Pure]
		public UISettings Clone()
		{
			return new UISettings
			{
				Language = Language,
				ThemeColor = ThemeColor
			};
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("language", Language ?? DefaultLanguage);
			writer.WriteAttributeColor("themecolor", ThemeColor);
		}

		public void Restore(XmlReader reader)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "language":
						var language = reader.ReadContentAsString();
						if (!string.IsNullOrEmpty(language))
							Language = language;
						break;

					case "themecolor":
						reader.ReadAttributeAsColor("themecolor", Log, DefaultThemeColor, out var themeColor);
						ThemeColor = themeColor;
						break;
				}
			}
		}
	}
}
