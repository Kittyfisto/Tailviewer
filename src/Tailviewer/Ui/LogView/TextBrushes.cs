using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows.Media;
using Tailviewer.Api;
using Tailviewer.Settings;

namespace Tailviewer.Ui.LogView
{
	public sealed class TextBrushes
	{
		private static readonly IReadOnlyList<LevelFlags> Levels = new[]
		{
			LevelFlags.Other,
			LevelFlags.Trace,
			LevelFlags.Debug,
			LevelFlags.Info,
			LevelFlags.Warning,
			LevelFlags.Error,
			LevelFlags.Fatal
		};

		public static Brush SelectedForegroundBrush { get; private set; }
		public static SolidColorBrush SelectedBackgroundBrush { get; private set; }
		public static SolidColorBrush SelectedUnfocusedBackgroundBrush { get; private set; }
		public static Brush HighlightedForegroundBrush { get; private set; }
		public static Brush HighlightedBackgroundBrush { get; private set; }
		public static Brush HighlightedSelectedForegroundBrush { get; private set; }
		public static Brush HighlightedSelectedBackgroundBrush { get; private set; }
		public static SolidColorBrush LineNumberForegroundBrush { get; private set; }
		public static SolidColorBrush DataSourceFilenameForegroundBrush { get; private set; }
		public static SolidColorBrush DataSourceCharacterCodeForegroundBrush { get; private set; }
		public static SolidColorBrush CanvasBackgroundBrush { get; private set; }
		public static SolidColorBrush DefaultForegroundBrush { get; private set; }
		public static SolidColorBrush DefaultBackgroundBrush { get; private set; }
		public static SolidColorBrush AlternatingBackgroundBrush { get; private set; }
		public static SolidColorBrush SeparatorBrush { get; private set; }

		private readonly Dictionary<LevelFlags, Brush> _foregroundBrushes;
		private readonly Dictionary<LevelFlags, Brush> _backgroundBrushes;
		private readonly Dictionary<LevelFlags, Brush> _alternateBackgroundBrushes;

		static TextBrushes()
		{
			SelectedBackgroundBrush = CreateBrush(UISettings.DefaultThemeColor);
			SelectedForegroundBrush = Brushes.White;
			SelectedUnfocusedBackgroundBrush = CreateBrush(Color.FromRgb(215, 215, 215));
			HighlightedForegroundBrush = Brushes.Black;
			HighlightedBackgroundBrush = CreateBrush(Color.FromRgb(255, 255, 77));
			HighlightedSelectedForegroundBrush = Brushes.Black;
			HighlightedSelectedBackgroundBrush = CreateBrush(Color.FromRgb(255, 150, 50));
			LineNumberForegroundBrush = CreateBrush(UISettings.DefaultThemeColor);
			DataSourceFilenameForegroundBrush = CreateBrush(Color.FromRgb(128, 128, 128));
			DataSourceCharacterCodeForegroundBrush = CreateBrush(UISettings.DefaultThemeColor);
			CanvasBackgroundBrush = CreateBrush(Colors.White);
			DefaultForegroundBrush = CreateBrush(Colors.Black);
			DefaultBackgroundBrush = CreateBrush(Colors.Transparent);
			AlternatingBackgroundBrush = CreateBrush(Color.FromRgb(0xE8, 0xF1, 0xF7));
			SeparatorBrush = CreateBrush(Color.FromRgb(0xE1, 0xE4, 0xE8));
		}

		/// <summary>
		///     Replaces the accent-derived brushes with new (frozen) instances for the
		///     given accent color. Replacing references (rather than mutating a shared
		///     Freezable) keeps every brush thread-safe.
		/// </summary>
		public static void UpdateTheme(Color primary)
		{
			SelectedBackgroundBrush = CreateBrush(primary);
			LineNumberForegroundBrush = CreateBrush(primary);
			DataSourceCharacterCodeForegroundBrush = CreateBrush(primary);
		}

		/// <summary>
		///     Replaces the neutral brushes with new (frozen) instances for the given
		///     light/dark mode.
		/// </summary>
		public static void UpdateNeutral(bool darkMode)
		{
			if (darkMode)
			{
				CanvasBackgroundBrush = CreateBrush(Color.FromRgb(0x1E, 0x1E, 0x1E));
				DefaultForegroundBrush = CreateBrush(Color.FromRgb(0xDC, 0xDC, 0xDC));
				DefaultBackgroundBrush = CreateBrush(Colors.Transparent);
				SelectedUnfocusedBackgroundBrush = CreateBrush(Color.FromRgb(0x3F, 0x3F, 0x46));
				AlternatingBackgroundBrush = CreateBrush(Color.FromRgb(0x25, 0x25, 0x26));
				DataSourceFilenameForegroundBrush = CreateBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));
				SeparatorBrush = CreateBrush(Color.FromRgb(0x3F, 0x3F, 0x46));
			}
			else
			{
				CanvasBackgroundBrush = CreateBrush(Colors.White);
				DefaultForegroundBrush = CreateBrush(Colors.Black);
				DefaultBackgroundBrush = CreateBrush(Colors.Transparent);
				SelectedUnfocusedBackgroundBrush = CreateBrush(Color.FromRgb(0xD7, 0xD7, 0xD7));
				AlternatingBackgroundBrush = CreateBrush(Color.FromRgb(0xE8, 0xF1, 0xF7));
				DataSourceFilenameForegroundBrush = CreateBrush(Color.FromRgb(0x80, 0x80, 0x80));
				SeparatorBrush = CreateBrush(Color.FromRgb(0xE1, 0xE4, 0xE8));
			}
		}

		public TextBrushes(ILogViewerSettings settings)
			: this(settings, LogLevelPalette.Light)
		{}

		public TextBrushes(ILogViewerSettings settings, LogLevelPalette palette)
		{
			_foregroundBrushes = new Dictionary<LevelFlags, Brush>();
			_backgroundBrushes = new Dictionary<LevelFlags, Brush>();
			_alternateBackgroundBrushes = new Dictionary<LevelFlags, Brush>();

			var defaults = LogLevelDefaults.For(palette);
			foreach (var level in Levels)
			{
				var levelSettings = settings != null ? GetLevelSettings(settings, level) : null;
				if (levelSettings != null && levelSettings.IsCustom)
				{
					_foregroundBrushes.Add(level, CreateBrush(levelSettings.ForegroundColor));
					_backgroundBrushes.Add(level, CreateBrush(levelSettings.BackgroundColor));
					_alternateBackgroundBrushes.Add(level, GetAlternatingBrush(levelSettings.BackgroundColor));
				}
				else
				{
					var levelDefaults = defaults.Get(level);
					_foregroundBrushes.Add(level, CreateBrush(levelDefaults.ForegroundColor));
					_backgroundBrushes.Add(level, CreateBrush(levelDefaults.BackgroundColor));
					_alternateBackgroundBrushes.Add(level, GetAlternatingBrush(levelDefaults.BackgroundColor));
				}
			}
		}

		public Brush ForegroundBrush(bool isSelected, bool isFocused, bool colorByLevel, LevelFlags level)
		{
			if (isSelected)
			{
				if (isFocused)
					return SelectedForegroundBrush;

				if (colorByLevel)
					return _foregroundBrushes[LevelFlags.Info];

				return DefaultForegroundBrush;
			}

			if (colorByLevel)
			{
				if (_foregroundBrushes.TryGetValue(level, out var brush))
					return brush;

				return _foregroundBrushes[LevelFlags.Other];
			}

			return DefaultForegroundBrush;
		}

		public Brush BackgroundBrush(bool isSelected, bool isFocused, bool colorByLevel, LevelFlags level, int lineIndex)
		{
			if (isSelected)
			{
				if (isFocused)
					return SelectedBackgroundBrush;

				return SelectedUnfocusedBackgroundBrush;
			}

			if (colorByLevel)
			{
				if (lineIndex % 2 == 0)
				{
					if (_backgroundBrushes.TryGetValue(level, out var brush))
						return brush;
				}

				if (_alternateBackgroundBrushes.TryGetValue(level, out var alternateBrush))
					return alternateBrush;
			}

			return null;
		}

		[Pure]
		private static SolidColorBrush CreateBrush(Color color)
		{
			var brush = new SolidColorBrush(color);
			brush.Freeze();
			return brush;
		}

		private static Brush GetAlternatingBrush(Color color)
		{
			if (color.A == 0 || Colors.White.Equals(color))
			{
				return AlternatingBackgroundBrush;
			}

			return CreateBrush(color);
		}

		private static LogLevelSettings GetLevelSettings(ILogViewerSettings settings, LevelFlags level)
		{
			switch (level)
			{
				case LevelFlags.Trace: return settings.Trace;
				case LevelFlags.Debug: return settings.Debug;
				case LevelFlags.Info: return settings.Info;
				case LevelFlags.Warning: return settings.Warning;
				case LevelFlags.Error: return settings.Error;
				case LevelFlags.Fatal: return settings.Fatal;
				default: return settings.Other;
			}
		}
	}
}
