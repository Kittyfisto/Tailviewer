using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows.Media;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.LogView
{
	public sealed class TextBrushes
	{
		public static readonly Brush SelectedForegroundBrush;
		public static readonly Brush SelectedBackgroundBrush;
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

		static TextBrushes()
		{
			SelectedBackgroundBrush = CreateBrush(Color.FromRgb(57, 152, 214));

			SelectedForegroundBrush = Brushes.White;

			SelectedUnfocusedBackgroundBrush = CreateBrush(Color.FromRgb(215, 215, 215));

			HighlightedForegroundBrush = Brushes.Black;
			HighlightedBackgroundBrush = CreateBrush(Color.FromRgb(255, 255, 77));

			HighlightedSelectedForegroundBrush = Brushes.Black;
			HighlightedSelectedBackgroundBrush = CreateBrush(Color.FromRgb(255, 150, 50));
			
			LineNumberForegroundBrush = CreateBrush(Color.FromRgb(43, 145, 175));

			DataSourceFilenameForegroundBrush = CreateBrush(Color.FromRgb(128, 128, 128));
			DataSourceCharacterCodeForegroundBrush = CreateBrush(Color.FromRgb(43, 145, 175));
		}

		public TextBrushes(ILogViewerSettings settings)
		{
			_foregroundBrushes = new Dictionary<LevelFlags, Brush>();
			_backgroundBrushes = new Dictionary<LevelFlags, Brush>();
			if (settings != null)
			{
				_foregroundBrushes.Add(LevelFlags.Other, CreateBrush(settings.Info.ForegroundColor));
				_backgroundBrushes.Add(LevelFlags.Other, CreateBrush(settings.Info.BackgroundColor));

				_foregroundBrushes.Add(LevelFlags.Trace, CreateBrush(settings.Trace.ForegroundColor));
				_backgroundBrushes.Add(LevelFlags.Trace, CreateBrush(settings.Trace.BackgroundColor));

				_foregroundBrushes.Add(LevelFlags.Debug, CreateBrush(settings.Debug.ForegroundColor));
				_backgroundBrushes.Add(LevelFlags.Debug, CreateBrush(settings.Debug.BackgroundColor));

				_foregroundBrushes.Add(LevelFlags.Info, CreateBrush(settings.Info.ForegroundColor));
				_backgroundBrushes.Add(LevelFlags.Info, CreateBrush(settings.Info.BackgroundColor));

				_foregroundBrushes.Add(LevelFlags.Warning, CreateBrush(settings.Warning.ForegroundColor));
				_backgroundBrushes.Add(LevelFlags.Warning, CreateBrush(settings.Warning.BackgroundColor));

				_foregroundBrushes.Add(LevelFlags.Error, CreateBrush(settings.Error.ForegroundColor));
				_backgroundBrushes.Add(LevelFlags.Error, CreateBrush(settings.Error.BackgroundColor));

				_foregroundBrushes.Add(LevelFlags.Fatal, CreateBrush(settings.Fatal.ForegroundColor));
				_backgroundBrushes.Add(LevelFlags.Fatal, CreateBrush(settings.Fatal.BackgroundColor));
			}
			else
			{
				foreach (LevelFlags level in Enum.GetValues(typeof(LevelFlags)))
				{
					_foregroundBrushes.Add(level, Brushes.Black);
					_backgroundBrushes.Add(level, Brushes.White);
				}
			}
		}

		public Brush ForegroundBrush(bool isSelected, bool isFocused, bool colorByLevel, LevelFlags level)
		{
			if (isSelected)
			{
				if (isFocused)
					return SelectedForegroundBrush;

				return _foregroundBrushes[LevelFlags.Info];
			}

			if (colorByLevel)
			{
				if (_foregroundBrushes.TryGetValue(level, out var brush))
					return brush;
			}

			/*if (textLine.IsHovered)
			{
				return TextHelper.HoveredForegroundBrush;
			}

			return TextHelper.NormalForegroundBrush;*/
			return _foregroundBrushes[LevelFlags.Info];
		}

		public Brush BackgroundBrush(bool isSelected, bool isFocused, bool colorByLevel, LevelFlags level)
		{
			if (isSelected)
			{
				if (isFocused)
					return SelectedBackgroundBrush;

				return SelectedUnfocusedBackgroundBrush;
			}

			if (colorByLevel)
			{
				if (_backgroundBrushes.TryGetValue(level, out var brush))
					return brush;
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
	}
}