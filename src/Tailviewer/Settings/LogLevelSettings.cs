using System.Reflection;
using System.Windows.Media;
using System.Xml;
using log4net;
using Tailviewer.Core;

namespace Tailviewer.Settings
{
	/// <summary>
	///     The (display) settings concerning a particular log level.
	/// </summary>
	public sealed class LogLevelSettings
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private Color _foregroundColor;
		private Color _backgroundColor;
		private bool _isCustom;

		public LogLevelSettings()
		{
			_foregroundColor = Colors.Black;
			_backgroundColor = Colors.Transparent;
		}

		/// <summary>
		///     The color of the foreground (e.g. the text) of a line with that log level.
		/// </summary>
		public Color ForegroundColor
		{
			get { return _foregroundColor; }
			set { _foregroundColor = value; }
		}

		/// <summary>
		///     The color of the background of a line with that log level.
		/// </summary>
		public Color BackgroundColor
		{
			get { return _backgroundColor; }
			set { _backgroundColor = value; }
		}

		/// <summary>
		///     Whether the user has explicitly configured this level's colors. When
		///     false, the level follows the current theme's default palette.
		/// </summary>
		public bool IsCustom
		{
			get { return _isCustom; }
			set { _isCustom = value; }
		}

		/// <summary>
		///     Compares this level's colors against the given foreground/background pair.
		/// </summary>
		public bool Matches(Color foregroundColor, Color backgroundColor)
		{
			return _foregroundColor == foregroundColor && _backgroundColor == backgroundColor;
		}

		public LogLevelSettings Clone()
		{
			return new LogLevelSettings
			{
				ForegroundColor = _foregroundColor,
				BackgroundColor = _backgroundColor,
				IsCustom = _isCustom
			};
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeColor("foregroundcolor", _foregroundColor);
			writer.WriteAttributeColor("backgroundcolor", _backgroundColor);
			writer.WriteAttributeString("iscustom", _isCustom ? "true" : "false");
		}

		/// <summary>
		///     Restores this level's colors and custom flag from the given reader.
		///     Returns true when an explicit <c>iscustom</c> attribute was present,
		///     false for legacy files that only contain the two colors.
		/// </summary>
		public bool Restore(XmlReader reader)
		{
			reader.ReadAttributeAsColor("foregroundcolor", Log, _foregroundColor, out _foregroundColor);
			reader.ReadAttributeAsColor("backgroundcolor", Log, _backgroundColor, out _backgroundColor);

			if (!reader.MoveToAttribute("iscustom"))
				return false;

			_isCustom = bool.TryParse(reader.ReadContentAsString(), out var isCustom) && isCustom;
			return true;
		}
	}
}