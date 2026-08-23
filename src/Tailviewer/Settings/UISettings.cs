using System;
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

		public static readonly Color DefaultThemeColor = Color.FromArgb(0xFF, 0x22, 0x22, 0x80);

		public string Language { get; set; }

		public Color ThemeColor { get; set; }

		public ThemeMode ThemeMode { get; set; }

		public UISettings()
		{
			Language = DefaultLanguage;
			ThemeColor = DefaultThemeColor;
			ThemeMode = ThemeMode.Light;
		}

		[Pure]
		public UISettings Clone()
		{
			return new UISettings
			{
				Language = Language,
				ThemeColor = ThemeColor,
				ThemeMode = ThemeMode
			};
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("language", Language ?? DefaultLanguage);
			writer.WriteAttributeColor("themecolor", ThemeColor);
			writer.WriteAttributeString("thememode", ThemeMode.ToString());
		}

		public void Restore(XmlReader reader)
		{
			bool themeModeAttributeSeen = false;

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

					case "thememode":
						themeModeAttributeSeen = true;
						if (Enum.TryParse(reader.ReadContentAsString(), true, out ThemeMode parsedMode) &&
						    Enum.IsDefined(typeof(ThemeMode), parsedMode))
						{
							ThemeMode = parsedMode;
						}
						break;

				}
			}

			if (!themeModeAttributeSeen)
			{
				reader.MoveToElement();
				for (int i = 0; i < reader.AttributeCount; ++i)
				{
					reader.MoveToAttribute(i);
					if (reader.Name == "darkmode")
					{
						ThemeMode = bool.TryParse(reader.ReadContentAsString(), out var darkMode) && darkMode
							? ThemeMode.Dark
							: ThemeMode.Light;
						break;
					}
				}
			}

		}
	}
}
