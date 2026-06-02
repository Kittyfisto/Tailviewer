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
		public static readonly Brush SelectedForegroundBrush;
		public static readonly Brush SelectedBackgroundBrush;
		public static readonly Brush SelectedUnfocusedForegroundBrush;
		public static readonly Brush SelectedUnfocusedBackgroundBrush;
		public static readonly Brush HighlightedForegroundBrush;
		public static readonly Brush HighlightedBackgroundBrush;
		public static readonly Brush HighlightedSelectedForegroundBrush;
		public static readonly Brush HighlightedSelectedBackgroundBrush;
		public static readonly Brush LineNumberForegroundBrush;
		public static readonly Brush DataSourceFilenameForegroundBrush;
		public static readonly Brush DataSourceCharacterCodeForegroundBrush;

		private readonly Dictionary<LevelFlags, Brush> _foregroundBrushes;
		private readonly Dictionary<LevelFlags, Brush> _backgroundBrushes;
		private readonly Dictionary<LevelFlags, Brush> _alternateBackgroundBrushes;

		static TextBrushes()
		{
			SelectedBackgroundBrush = CreateBrush(Color.FromRgb(57, 152, 214));

			SelectedForegroundBrush = Brushes.White;

			SelectedUnfocusedBackgroundBrush = CreateBrush(Themes.AppTheme.SelectedUnfocusedBackgroundColor);
			SelectedUnfocusedForegroundBrush = CreateBrush(Themes.AppTheme.LogForegroundColor);

			HighlightedForegroundBrush = Brushes.Black;
			HighlightedBackgroundBrush = CreateBrush(Color.FromRgb(255, 255, 77));

			HighlightedSelectedForegroundBrush = Brushes.Black;
			HighlightedSelectedBackgroundBrush = CreateBrush(Color.FromRgb(255, 150, 50));
			
			LineNumberForegroundBrush = CreateBrush(Themes.AppTheme.LineNumberForegroundColor);

			DataSourceFilenameForegroundBrush = CreateBrush(Color.FromRgb(128, 128, 128));
			DataSourceCharacterCodeForegroundBrush = CreateBrush(Themes.AppTheme.LineNumberForegroundColor);
		}

		public TextBrushes(ILogViewerSettings settings)
		{
			_foregroundBrushes = new Dictionary<LevelFlags, Brush>();
			_backgroundBrushes = new Dictionary<LevelFlags, Brush>();
			_alternateBackgroundBrushes = new Dictionary<LevelFlags, Brush>();
			if (settings != null)
			{
				_foregroundBrushes.Add(LevelFlags.Other, CreateBrush(AdaptForeground(settings.Other.ForegroundColor)));
				_backgroundBrushes.Add(LevelFlags.Other, CreateBrush(settings.Other.BackgroundColor));
				_alternateBackgroundBrushes.Add(LevelFlags.Other, CreateBrush(GetAlternatingColor(settings.Other.BackgroundColor)));

				_foregroundBrushes.Add(LevelFlags.Trace, CreateBrush(AdaptForeground(settings.Trace.ForegroundColor)));
				_backgroundBrushes.Add(LevelFlags.Trace, CreateBrush(settings.Trace.BackgroundColor));
				_alternateBackgroundBrushes.Add(LevelFlags.Trace, CreateBrush(GetAlternatingColor(settings.Trace.BackgroundColor)));

				_foregroundBrushes.Add(LevelFlags.Debug, CreateBrush(AdaptForeground(settings.Debug.ForegroundColor)));
				_backgroundBrushes.Add(LevelFlags.Debug, CreateBrush(settings.Debug.BackgroundColor));
				_alternateBackgroundBrushes.Add(LevelFlags.Debug, CreateBrush(GetAlternatingColor(settings.Debug.BackgroundColor)));

				_foregroundBrushes.Add(LevelFlags.Info, CreateBrush(AdaptForeground(settings.Info.ForegroundColor)));
				_backgroundBrushes.Add(LevelFlags.Info, CreateBrush(settings.Info.BackgroundColor));
				_alternateBackgroundBrushes.Add(LevelFlags.Info, CreateBrush(GetAlternatingColor(settings.Info.BackgroundColor)));

				_foregroundBrushes.Add(LevelFlags.Warning, CreateBrush(settings.Warning.ForegroundColor));
				_backgroundBrushes.Add(LevelFlags.Warning, CreateBrush(settings.Warning.BackgroundColor));
				_alternateBackgroundBrushes.Add(LevelFlags.Warning, CreateBrush(GetAlternatingColor(settings.Warning.BackgroundColor)));

				_foregroundBrushes.Add(LevelFlags.Error, CreateBrush(settings.Error.ForegroundColor));
				_backgroundBrushes.Add(LevelFlags.Error, CreateBrush(settings.Error.BackgroundColor));
				_alternateBackgroundBrushes.Add(LevelFlags.Error, CreateBrush(GetAlternatingColor(settings.Error.BackgroundColor)));

				_foregroundBrushes.Add(LevelFlags.Fatal, CreateBrush(settings.Fatal.ForegroundColor));
				_backgroundBrushes.Add(LevelFlags.Fatal, CreateBrush(settings.Fatal.BackgroundColor));
				_alternateBackgroundBrushes.Add(LevelFlags.Fatal, CreateBrush(GetAlternatingColor(settings.Fatal.BackgroundColor)));
			}
			else
			{
				var foreground = CreateBrush(Themes.AppTheme.LogForegroundColor);
				foreach (LevelFlags level in Enum.GetValues(typeof(LevelFlags)))
				{
					_foregroundBrushes.Add(level, foreground);
					_backgroundBrushes.Add(level, Themes.AppTheme.LogBackgroundBrush);
					_alternateBackgroundBrushes.Add(level, Themes.AppTheme.LogBackgroundBrush);
				}
			}
		}

		public Brush ForegroundBrush(bool isSelected, bool isFocused, bool colorByLevel, LevelFlags level)
		{
			if (isSelected)
			{
				if (isFocused)
					return SelectedForegroundBrush;

				return SelectedUnfocusedForegroundBrush;
			}

			if (colorByLevel)
			{
				if (_foregroundBrushes.TryGetValue(level, out var brush))
					return brush;
			}

			return _foregroundBrushes[LevelFlags.Other];
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
		private static Brush CreateBrush(Color color)
		{
			var brush = new SolidColorBrush(color);
			brush.Freeze();
			return brush;
		}

		[Pure]
		private static Color GetAlternatingColor(Color color)
		{
			if (color.A == 0 || Colors.White.Equals(color))
			{
				return Themes.AppTheme.LogAlternatingBackgroundColor;
			}

			return color;
		}

		[Pure]
		private static Color AdaptForeground(Color color)
		{
			// User-customized colors are kept as-is. Only the default black-on-light
			// foreground is remapped so that text stays readable on a dark background.
			if (Themes.AppTheme.IsDark && Colors.Black.Equals(color))
			{
				return Themes.AppTheme.LogForegroundColor;
			}

			return color;
		}
	}
}