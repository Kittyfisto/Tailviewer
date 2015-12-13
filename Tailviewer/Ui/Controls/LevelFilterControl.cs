using System.Windows;
using System.Windows.Controls;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui.Controls
{
	public class LevelFilterControl : Control
	{
		public static readonly DependencyProperty ShowFatalProperty =
			DependencyProperty.Register("ShowFatal", typeof (bool), typeof (LevelFilterControl),
			                            new PropertyMetadata(false, OnFatalChanged));

		public static readonly DependencyProperty ShowErrorProperty =
			DependencyProperty.Register("ShowError", typeof (bool), typeof (LevelFilterControl),
			                            new PropertyMetadata(false, OnErrorChanged));

		public static readonly DependencyProperty ShowWarningProperty =
			DependencyProperty.Register("ShowWarning", typeof (bool), typeof (LevelFilterControl),
			                            new PropertyMetadata(false, OnWarningChanged));

		public static readonly DependencyProperty ShowInfoProperty =
			DependencyProperty.Register("ShowInfo", typeof (bool), typeof (LevelFilterControl),
			                            new PropertyMetadata(false, OnInfoChanged));

		public static readonly DependencyProperty ShowDebugProperty =
			DependencyProperty.Register("ShowDebug", typeof (bool), typeof (LevelFilterControl),
										new PropertyMetadata(false, OnDebugChanged));

		public static readonly DependencyProperty LevelsFilterProperty =
			DependencyProperty.Register("LevelsFilter", typeof (LevelFlags), typeof (LevelFilterControl),
			                            new PropertyMetadata(default(LevelFlags), OnLevelsChanged));

		static LevelFilterControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (LevelFilterControl),
			                                         new FrameworkPropertyMetadata(typeof (LevelFilterControl)));
		}

		public LevelFlags LevelsFilter
		{
			get { return (LevelFlags) GetValue(LevelsFilterProperty); }
			set { SetValue(LevelsFilterProperty, value); }
		}

		public bool ShowDebug
		{
			get { return (bool) GetValue(ShowDebugProperty); }
			set { SetValue(ShowDebugProperty, value); }
		}

		public bool ShowInfo
		{
			get { return (bool) GetValue(ShowInfoProperty); }
			set { SetValue(ShowInfoProperty, value); }
		}

		public bool ShowWarning
		{
			get { return (bool) GetValue(ShowWarningProperty); }
			set { SetValue(ShowWarningProperty, value); }
		}

		public bool ShowError
		{
			get { return (bool) GetValue(ShowErrorProperty); }
			set { SetValue(ShowErrorProperty, value); }
		}

		public bool ShowFatal
		{
			get { return (bool) GetValue(ShowFatalProperty); }
			set { SetValue(ShowFatalProperty, value); }
		}

		private static void OnFatalChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LevelFilterControl) dependencyObject).OnFatalChanged((bool) args.NewValue);
		}

		private void OnFatalChanged(bool isChecked)
		{
			const LevelFlags level = LevelFlags.Fatal;
			if (isChecked)
			{
				LevelsFilter |= level;
			}
			else
			{
				LevelsFilter &= ~level;
			}
		}

		private static void OnErrorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LevelFilterControl) dependencyObject).OnErrorChanged((bool) args.NewValue);
		}

		private void OnErrorChanged(bool isChecked)
		{
			const LevelFlags level = LevelFlags.Error;
			if (isChecked)
			{
				LevelsFilter |= level;
			}
			else
			{
				LevelsFilter &= ~level;
			}
		}

		private static void OnWarningChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LevelFilterControl) dependencyObject).OnWarningChanged((bool) args.NewValue);
		}

		private void OnWarningChanged(bool isChecked)
		{
			const LevelFlags level = LevelFlags.Warning;
			if (isChecked)
			{
				LevelsFilter |= level;
			}
			else
			{
				LevelsFilter &= ~level;
			}
		}

		private static void OnInfoChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LevelFilterControl) dependencyObject).OnInfoChanged((bool) args.NewValue);
		}

		private void OnInfoChanged(bool isChecked)
		{
			const LevelFlags level = LevelFlags.Info;
			if (isChecked)
			{
				LevelsFilter |= level;
			}
			else
			{
				LevelsFilter &= ~level;
			}
		}

		private static void OnDebugChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LevelFilterControl) dependencyObject).OnDebugChanged((bool) args.NewValue);
		}

		private void OnDebugChanged(bool isChecked)
		{
			const LevelFlags level = LevelFlags.Debug;
			if (isChecked)
			{
				LevelsFilter |= level;
			}
			else
			{
				LevelsFilter &= ~level;
			}
		}

		private static void OnLevelsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LevelFilterControl) dependencyObject).OnLevelsChanged((LevelFlags) args.NewValue);
		}

		private void OnLevelsChanged(LevelFlags levels)
		{
			ShowFatal = levels.HasFlag(LevelFlags.Fatal);
			ShowError = levels.HasFlag(LevelFlags.Error);
			ShowWarning = levels.HasFlag(LevelFlags.Warning);
			ShowInfo = levels.HasFlag(LevelFlags.Info);
			ShowDebug = levels.HasFlag(LevelFlags.Debug);
		}
	}
}