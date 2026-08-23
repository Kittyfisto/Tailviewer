using System.Windows.Media;
using Tailviewer.Api;

namespace Tailviewer.Settings
{
	/// <summary>
	///     The light and dark default palettes for the individual log levels.
	/// </summary>
	public sealed class LogLevelDefaults
	{
		public LogLevelSettings Other { get; }
		public LogLevelSettings Trace { get; }
		public LogLevelSettings Debug { get; }
		public LogLevelSettings Info { get; }
		public LogLevelSettings Warning { get; }
		public LogLevelSettings Error { get; }
		public LogLevelSettings Fatal { get; }

		private LogLevelDefaults(
			LogLevelSettings other,
			LogLevelSettings trace,
			LogLevelSettings debug,
			LogLevelSettings info,
			LogLevelSettings warning,
			LogLevelSettings error,
			LogLevelSettings fatal)
		{
			Other = other;
			Trace = trace;
			Debug = debug;
			Info = info;
			Warning = warning;
			Error = error;
			Fatal = fatal;
		}

		public static LogLevelDefaults Light { get; } = CreateLight();

		public static LogLevelDefaults Dark { get; } = CreateDark();

		public static LogLevelDefaults For(LogLevelPalette palette)
		{
			return palette == LogLevelPalette.Dark ? Dark : Light;
		}

		public static LogLevelDefaults For(bool darkMode)
		{
			return darkMode ? Dark : Light;
		}

		public LogLevelSettings Get(LevelFlags level)
		{
			switch (level)
			{
				case LevelFlags.Trace: return Trace;
				case LevelFlags.Debug: return Debug;
				case LevelFlags.Info: return Info;
				case LevelFlags.Warning: return Warning;
				case LevelFlags.Error: return Error;
				case LevelFlags.Fatal: return Fatal;
				default: return Other;
			}
		}

		private static LogLevelSettings Level(Color foreground)
		{
			return new LogLevelSettings
			{
				ForegroundColor = foreground,
				BackgroundColor = Colors.Transparent
			};
		}

		private static LogLevelSettings Level(Color foreground, Color background)
		{
			return new LogLevelSettings
			{
				ForegroundColor = foreground,
				BackgroundColor = background
			};
		}

		private static LogLevelDefaults CreateLight()
		{
			return new LogLevelDefaults(
				Level(Color.FromRgb(0x7C, 0x6E, 0x9E)),
				Level(Color.FromRgb(0x9A, 0xA5, 0xB1)),
				Level(Color.FromRgb(0x6B, 0x7A, 0x8F)),
				Level(Color.FromRgb(0x33, 0x33, 0x33)),
				Level(Color.FromRgb(0x8A, 0x4D, 0x00), Color.FromRgb(0xFF, 0xF3, 0xD6)),
				Level(Color.FromRgb(0xB3, 0x26, 0x1E), Color.FromRgb(0xFD, 0xEC, 0xEA)),
				Level(Color.FromRgb(0xFF, 0xFF, 0xFF), Color.FromRgb(0xC5, 0x22, 0x1F)));
		}

		private static LogLevelDefaults CreateDark()
		{
			return new LogLevelDefaults(
				Level(Color.FromRgb(0x8E, 0x7F, 0xAE)),
				Level(Color.FromRgb(0x5B, 0x64, 0x72)),
				Level(Color.FromRgb(0x7E, 0x8C, 0x9E)),
				Level(Color.FromRgb(0xDC, 0xDC, 0xDC)),
				Level(Color.FromRgb(0xF5, 0xD7, 0x7E), Color.FromRgb(0x5A, 0x47, 0x00)),
				Level(Color.FromRgb(0xFF, 0xB4, 0xAB), Color.FromRgb(0x6B, 0x1F, 0x1F)),
				Level(Color.FromRgb(0xFF, 0xFF, 0xFF), Color.FromRgb(0x9E, 0x1B, 0x1B)));
		}
	}
}