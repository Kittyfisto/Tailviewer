using log4net;
using Metrolib;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;
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

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private int _linesScrolledPerWheelTick;
		private int _fontSize;

		public LogViewerSettings()
		{
			_linesScrolledPerWheelTick = DefaultLinesScrolledPerWheelTick;
			_fontSize = DefaultFontSize;
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

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeInt("linesscrolledperwheeltick", _linesScrolledPerWheelTick);
			writer.WriteAttributeInt("fontsize", _fontSize);
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
		}

		[Pure]
		public LogViewerSettings Clone()
		{
			return new LogViewerSettings
			{
				LinesScrolledPerWheelTick = LinesScrolledPerWheelTick,
				FontSize = FontSize
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}