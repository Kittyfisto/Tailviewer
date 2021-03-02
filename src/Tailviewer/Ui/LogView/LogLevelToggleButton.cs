using System.Windows;
using System.Windows.Media;
using Metrolib;
using Tailviewer.Api;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	///     Quickly filters / shows log entries with the specified log level.
	/// </summary>
	public sealed class LogLevelToggleButton
		: ToolbarToggleButton
	{
		public static readonly DependencyProperty LogLevelProperty = DependencyProperty.Register(
		 "LogLevel", typeof(LevelFlags), typeof(LogLevelToggleButton),
		 new PropertyMetadata(default(LevelFlags), OnLogLevelChanged));

		static LogLevelToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LogLevelToggleButton),
			                                         new FrameworkPropertyMetadata(typeof(LogLevelToggleButton)));
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
			CheckedIcon = GetCheckedIcon(logLevel);
			UncheckedIcon = GetUncheckedIcon(logLevel);
		}

		private Geometry GetCheckedIcon(LevelFlags logLevel)
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
				case LevelFlags.All: return Icons.SetAll;
				default:
					return null;
			}
		}

		private Geometry GetUncheckedIcon(LevelFlags logLevel)
		{
			switch (logLevel)
			{
				case LevelFlags.Other: return null;
				case LevelFlags.Trace: return Icons.ChatRemoveOutline;
				case LevelFlags.Debug: return Icons.BugRemoveOutline;
				case LevelFlags.Info: return Icons.InformationRemoveOutline;
				case LevelFlags.Warning: return Icons.AlertRemoveOutline;
				case LevelFlags.Error: return Icons.AlertCircleRemoveOutline;
				case LevelFlags.Fatal: return Icons.AlertRhombusRemoveOutline;
				case LevelFlags.All: return Icons.SetNone;
				default:
					return null;
			}
		}
	}
}