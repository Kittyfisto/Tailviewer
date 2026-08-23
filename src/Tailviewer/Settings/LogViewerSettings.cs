using log4net;
using Metrolib;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Windows.Media;
using System.Xml;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.Settings
{
	public sealed class LogViewerSettings
		: ILogViewerSettings
		, ICloneable
	{
		public const int DefaultLinesScrolledPerWheelTick = 2;
		public const int DefaultFontSize = 12;
		public const int DefaultTabWidth = 4;

		public static LogLevelSettings DefaultOther => LogLevelDefaults.Light.Get(LevelFlags.Other).Clone();
		public static LogLevelSettings DefaultTrace => LogLevelDefaults.Light.Get(LevelFlags.Trace).Clone();
		public static LogLevelSettings DefaultDebug => LogLevelDefaults.Light.Get(LevelFlags.Debug).Clone();
		public static LogLevelSettings DefaultInfo => LogLevelDefaults.Light.Get(LevelFlags.Info).Clone();
		public static LogLevelSettings DefaultWarning => LogLevelDefaults.Light.Get(LevelFlags.Warning).Clone();
		public static LogLevelSettings DefaultError => LogLevelDefaults.Light.Get(LevelFlags.Error).Clone();
		public static LogLevelSettings DefaultFatal => LogLevelDefaults.Light.Get(LevelFlags.Fatal).Clone();

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private int _linesScrolledPerWheelTick;
		private int _fontSize;
		private int _tabWidth;
		private LogLevelSettings _other;
		private LogLevelSettings _trace;
		private LogLevelSettings _debug;
		private LogLevelSettings _info;
		private LogLevelSettings _warning;
		private LogLevelSettings _error;
		private LogLevelSettings _fatal;

		public LogViewerSettings()
		{
			_linesScrolledPerWheelTick = DefaultLinesScrolledPerWheelTick;
			_fontSize = DefaultFontSize;
			_tabWidth = DefaultTabWidth;
			_other = DefaultOther;
			_trace = DefaultTrace;
			_debug = DefaultDebug;
			_info = DefaultInfo;
			_warning = DefaultWarning;
			_error = DefaultError;
			_fatal = DefaultFatal;
		}

		public int LinesScrolledPerWheelTick
		{
			get { return _linesScrolledPerWheelTick; }
			set { _linesScrolledPerWheelTick = value; }
		}

		public int FontSize
		{
			get { return _fontSize; }
			set { _fontSize = value; }
		}

		public int TabWidth
		{
			get { return _tabWidth; }
			set { _tabWidth = value; }
		}

		public LogLevelSettings Other
		{
			get { return _other; }
		}

		public LogLevelSettings Trace
		{
			get { return _trace; }
		}

		public LogLevelSettings Debug
		{
			get { return _debug; }
		}

		public LogLevelSettings Info
		{
			get { return _info; }
		}

		public LogLevelSettings Warning
		{
			get { return _warning; }
		}

		public LogLevelSettings Error
		{
			get { return _error; }
		}

		public LogLevelSettings Fatal
		{
			get { return _fatal; }
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeInt("linesscrolledperwheeltick", _linesScrolledPerWheelTick);
			writer.WriteAttributeInt("fontsize", _fontSize);
			writer.WriteAttributeInt("tabwidth", _tabWidth);

			writer.WriteStartElement("other");
			_other.Save(writer);
			writer.WriteEndElement();

			writer.WriteStartElement("trace");
			_trace.Save(writer);
			writer.WriteEndElement();

			writer.WriteStartElement("debug");
			_debug.Save(writer);
			writer.WriteEndElement();

			writer.WriteStartElement("info");
			_info.Save(writer);
			writer.WriteEndElement();

			writer.WriteStartElement("warning");
			_warning.Save(writer);
			writer.WriteEndElement();

			writer.WriteStartElement("error");
			_error.Save(writer);
			writer.WriteEndElement();

			writer.WriteStartElement("fatal");
			_fatal.Save(writer);
			writer.WriteEndElement();
		}

		public void Restore(XmlReader reader)
		{
			reader.ReadAttributeAsInt("linesscrolledperwheeltick", Log, DefaultLinesScrolledPerWheelTick, out _linesScrolledPerWheelTick);
			if (_linesScrolledPerWheelTick < 1)
			{
				Log.WarnFormat("Attribute 'linesscrolledperwheeltick' must be 1 or greater, restoring value to default...");
				_linesScrolledPerWheelTick = DefaultLinesScrolledPerWheelTick;
			}

			reader.ReadAttributeAsInt("fontsize", Log, DefaultFontSize, out _fontSize);
			if (_fontSize < 1)
			{
				Log.WarnFormat("Attribute 'fontsize' must be 1 or greater, restoring value to default...");
				_fontSize = DefaultFontSize;
			}

			reader.ReadAttributeAsInt("tabwidth", Log, DefaultTabWidth, out _tabWidth);
			if (_tabWidth < 1)
			{
				Log.WarnFormat("Attribute 'tabwidth' must be 1 or greater, restoring value to default...");
				_tabWidth = DefaultTabWidth;
			}

			reader.MoveToElement();
			var subtree = reader.ReadSubtree();
			while (subtree.Read())
			{
				switch (subtree.Name)
				{
					case "other":
						RestoreLevel(_other, subtree, LevelFlags.Other);
						break;

					case "trace":
						RestoreLevel(_trace, subtree, LevelFlags.Trace);
						break;

					case "debug":
						RestoreLevel(_debug, subtree, LevelFlags.Debug);
						break;

					case "info":
						RestoreLevel(_info, subtree, LevelFlags.Info);
						break;

					case "warning":
						RestoreLevel(_warning, subtree, LevelFlags.Warning);
						break;

					case "error":
						RestoreLevel(_error, subtree, LevelFlags.Error);
						break;

					case "fatal":
						RestoreLevel(_fatal, subtree, LevelFlags.Fatal);
						break;
				}
			}
		}

		/// <summary>
		///     Restores a single level, inferring the legacy custom flag when the
		///     settings file predates the <c>iscustom</c> attribute.
		/// </summary>
		private void RestoreLevel(LogLevelSettings level, XmlReader reader, LevelFlags flag)
		{
			if (!level.Restore(reader))
			{
				var light = LogLevelDefaults.Light.Get(flag);
				level.IsCustom = !level.Matches(light.ForegroundColor, light.BackgroundColor);
			}
		}

		/// <summary>
		///     Applies the given theme's default palette to every level that has not
		///     been customized by the user. Custom levels are left untouched.
		/// </summary>
		public void ApplyThemeDefaults(bool darkMode)
		{
			var defaults = LogLevelDefaults.For(darkMode);
			ApplyLevelDefault(_other, defaults.Other);
			ApplyLevelDefault(_trace, defaults.Trace);
			ApplyLevelDefault(_debug, defaults.Debug);
			ApplyLevelDefault(_info, defaults.Info);
			ApplyLevelDefault(_warning, defaults.Warning);
			ApplyLevelDefault(_error, defaults.Error);
			ApplyLevelDefault(_fatal, defaults.Fatal);
		}

		/// <summary>
		///     Restores the persisted light-palette baseline and makes every log level
		///     follow the currently active light/dark theme again.
		/// </summary>
		public void RestoreDefaultColors()
		{
			RestoreLevelDefault(_other, LogLevelDefaults.Light.Other);
			RestoreLevelDefault(_trace, LogLevelDefaults.Light.Trace);
			RestoreLevelDefault(_debug, LogLevelDefaults.Light.Debug);
			RestoreLevelDefault(_info, LogLevelDefaults.Light.Info);
			RestoreLevelDefault(_warning, LogLevelDefaults.Light.Warning);
			RestoreLevelDefault(_error, LogLevelDefaults.Light.Error);
			RestoreLevelDefault(_fatal, LogLevelDefaults.Light.Fatal);
		}

		private static void ApplyLevelDefault(LogLevelSettings level, LogLevelSettings defaults)
		{
			if (level.IsCustom)
				return;

			level.ForegroundColor = defaults.ForegroundColor;
			level.BackgroundColor = defaults.BackgroundColor;
		}

		private static void RestoreLevelDefault(LogLevelSettings level, LogLevelSettings defaults)
		{
			level.ForegroundColor = defaults.ForegroundColor;
			level.BackgroundColor = defaults.BackgroundColor;
			level.IsCustom = false;
		}

		[Pure]
		public LogViewerSettings Clone()
		{
			return new LogViewerSettings
			{
				LinesScrolledPerWheelTick = LinesScrolledPerWheelTick,
				FontSize = FontSize,
				TabWidth = TabWidth,
				_other = Other.Clone(),
				_trace = Trace.Clone(),
				_debug = Debug.Clone(),
				_info = Info.Clone(),
				_warning = Warning.Clone(),
				_error = Error.Clone(),
				_fatal = Fatal.Clone()
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}
