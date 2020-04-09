using log4net;
using Metrolib;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Windows.Media;
using System.Xml;
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

		public static LogLevelSettings DefaultTrace => new LogLevelSettings
		{
			ForegroundColor = Color.FromRgb(128, 128, 128),
			BackgroundColor = Colors.Transparent
		};

		public static LogLevelSettings DefaultDebug => new LogLevelSettings
		{
			ForegroundColor = Color.FromRgb(128, 128, 128),
			BackgroundColor = Colors.Transparent
		};

		public static LogLevelSettings DefaultInfo => new LogLevelSettings
		{
			ForegroundColor = Colors.Black,
			BackgroundColor = Colors.Transparent
		};

		public static LogLevelSettings DefaultWarning => new LogLevelSettings
		{
			ForegroundColor = Colors.White,
			BackgroundColor = Color.FromRgb(255, 195, 0)
		};

		public static LogLevelSettings DefaultError => new LogLevelSettings
		{
			ForegroundColor = Colors.White,
			BackgroundColor = Color.FromRgb(232, 17, 35)
		};

		public static LogLevelSettings DefaultFatal => new LogLevelSettings
		{
			ForegroundColor = Colors.White,
			BackgroundColor = Color.FromRgb(232, 17, 35)
		};

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private int _linesScrolledPerWheelTick;
		private int _fontSize;
		private int _tabWidth;
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
					case "trace":
						_trace.Restore(subtree);
						break;

					case "debug":
						_debug.Restore(subtree);
						break;

					case "info":
						_info.Restore(subtree);
						break;

					case "warning":
						_warning.Restore(subtree);
						break;

					case "error":
						_error.Restore(subtree);
						break;

					case "fatal":
						_fatal.Restore(subtree);
						break;
				}
			}
		}

		[Pure]
		public LogViewerSettings Clone()
		{
			return new LogViewerSettings
			{
				LinesScrolledPerWheelTick = LinesScrolledPerWheelTick,
				FontSize = FontSize,
				TabWidth = TabWidth,
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