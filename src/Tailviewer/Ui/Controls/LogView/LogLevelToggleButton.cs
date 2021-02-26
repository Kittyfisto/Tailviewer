using System.Windows;
using System.Windows.Media;
using Metrolib;
using Metrolib.Controls;

namespace Tailviewer.Ui.Controls.LogView
{
	/// <summary>
	///     Quickly filters / shows log entries with the specified log level.
	/// </summary>
	public sealed class LogLevelToggleButton
		: FlatToggleButton
	{
		public static readonly DependencyProperty LogLevelProperty = DependencyProperty.Register(
		 "LogLevel", typeof(LevelFlags), typeof(LogLevelToggleButton),
		 new PropertyMetadata(default(LevelFlags), OnLogLevelChanged));

		private static readonly DependencyPropertyKey IconOutlinePropertyKey
			= DependencyProperty.RegisterReadOnly("IconOutline", typeof(Geometry), typeof(LogLevelToggleButton),
			                                      new FrameworkPropertyMetadata(default(Geometry),
				                                                                    FrameworkPropertyMetadataOptions
					                                                                    .None));

		public static readonly DependencyProperty IconOutlineProperty
			= IconOutlinePropertyKey.DependencyProperty;

		private static readonly DependencyPropertyKey IconBackgroundPropertyKey
			= DependencyProperty.RegisterReadOnly("IconBackground", typeof(Geometry), typeof(LogLevelToggleButton),
			                                      new FrameworkPropertyMetadata(default(Geometry),
				                                                                    FrameworkPropertyMetadataOptions
					                                                                    .None));

		public static readonly DependencyProperty IconBackgroundProperty
			= IconBackgroundPropertyKey.DependencyProperty;

		public static readonly DependencyProperty IconBackgroundBrushProperty = DependencyProperty.Register(
		 "IconBackgroundBrush", typeof(Brush), typeof(LogLevelToggleButton), new PropertyMetadata(default(Brush)));

		static LogLevelToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LogLevelToggleButton),
			                                         new FrameworkPropertyMetadata(typeof(LogLevelToggleButton)));
		}

		public Brush IconBackgroundBrush
		{
			get { return (Brush) GetValue(IconBackgroundBrushProperty); }
			set { SetValue(IconBackgroundBrushProperty, value); }
		}

		public Geometry IconBackground
		{
			get { return (Geometry) GetValue(IconBackgroundProperty); }
			private set { SetValue(IconBackgroundPropertyKey, value); }
		}

		public Geometry IconOutline
		{
			get { return (Geometry) GetValue(IconOutlineProperty); }
			private set { SetValue(IconOutlinePropertyKey, value); }
		}

		public LevelFlags LogLevel
		{
			get { return (LevelFlags) GetValue(LogLevelProperty); }
			set { SetValue(LogLevelProperty, value); }
		}

		private static void OnLogLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((LogLevelToggleButton) d).OnLogLevelChanged((LevelFlags) e.NewValue);
		}

		private void OnLogLevelChanged(LevelFlags logLevel)
		{
			IconOutline = GetIconOutline(logLevel);
			IconBackground = GetIconBackground(logLevel);
		}

		private Geometry GetIconOutline(LevelFlags logLevel)
		{
			switch (logLevel)
			{
				case LevelFlags.Other: return null;
				case LevelFlags.Trace: return Icons.ChatOutline;
				case LevelFlags.Debug: return Icons.BugOutline;
				case LevelFlags.Info: return Icons.InformationOutline;
				case LevelFlags.Warning: return Icons.AlertOutline;
				case LevelFlags.Error: return Icons.AlertCircleOutline;
				case LevelFlags.Fatal: return Icons.AlertRhombusOutline;
				case LevelFlags.All: return null;
				default:
					return null;
			}
		}

		private Geometry GetIconBackground(LevelFlags logLevel)
		{
			switch (logLevel)
			{
				case LevelFlags.Other: return null;
				case LevelFlags.Trace: return Icons.Chat;
				case LevelFlags.Debug: return Icons.Bug;
				case LevelFlags.Info: return Icons.Information;
				case LevelFlags.Warning: return Icons.Alert;
				case LevelFlags.Error: return Icons.AlertCircle;
				case LevelFlags.Fatal: return Icons.AlertRhombus;
				case LevelFlags.All: return null;
				default:
					return null;
			}
		}
	}
}