using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Xml;
using log4net;
using Metrolib;
using Tailviewer.Core;

namespace Tailviewer.Settings
{
	public sealed class LogViewerSettings
		: ILogViewerSettings
		, ICloneable
	{
		public const int DefaultLinesScrolledPerWheelTick = 2;

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private int _linesScrolledPerWheelTick;

		public LogViewerSettings()
		{
			_linesScrolledPerWheelTick = DefaultLinesScrolledPerWheelTick;
		}

		public int LinesScrolledPerWheelTick
		{
			get { return _linesScrolledPerWheelTick; }
			set { _linesScrolledPerWheelTick = value; }
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeInt("linesscrolledperwheeltick", _linesScrolledPerWheelTick);
		}

		public void Restore(XmlReader reader)
		{
			reader.ReadAttributeAsInt("linesscrolledperwheeltick", Log, DefaultLinesScrolledPerWheelTick, out _linesScrolledPerWheelTick);

			if (_linesScrolledPerWheelTick < 1)
			{
				Log.WarnFormat("Attribute 'linesscrolledperwheeltick' must be 1 or greater, restoring value to default...");
				_linesScrolledPerWheelTick = DefaultLinesScrolledPerWheelTick;
			}
		}

		[Pure]
		public LogViewerSettings Clone()
		{
			return new LogViewerSettings
			{
				LinesScrolledPerWheelTick = LinesScrolledPerWheelTick
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}