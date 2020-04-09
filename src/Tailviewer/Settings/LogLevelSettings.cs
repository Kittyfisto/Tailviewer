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

		public LogLevelSettings Clone()
		{
			return new LogLevelSettings
			{
				ForegroundColor = _foregroundColor,
				BackgroundColor = _backgroundColor
			};
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeColor("foregroundcolor", _foregroundColor);
			writer.WriteAttributeColor("backgroundcolor", _backgroundColor);
		}

		public void Restore(XmlReader reader)
		{
			reader.ReadAttributeAsColor("foregroundcolor", Log, _foregroundColor, out _foregroundColor);
			reader.ReadAttributeAsColor("backgroundcolor", Log, _backgroundColor, out _backgroundColor);
		}
	}
}