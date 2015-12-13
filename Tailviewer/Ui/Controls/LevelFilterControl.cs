using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Tailviewer.BusinessLogic;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls
{
	internal class LevelFilterControl : Control
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

		public static readonly DependencyProperty ShowOtherProperty =
			DependencyProperty.Register("ShowOther", typeof(bool), typeof(LevelFilterControl),
										new PropertyMetadata(false, OnShowOtherChanged));

		public static readonly DependencyProperty DataSourceProperty =
			DependencyProperty.Register("DataSource", typeof (DataSourceViewModel), typeof (LevelFilterControl),
			                            new PropertyMetadata(null, OnDataSourceChanged));

		static LevelFilterControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (LevelFilterControl),
			                                         new FrameworkPropertyMetadata(typeof (LevelFilterControl)));
		}

		public DataSourceViewModel DataSource
		{
			get { return (DataSourceViewModel) GetValue(DataSourceProperty); }
			set { SetValue(DataSourceProperty, value); }
		}

		public bool ShowOther
		{
			get { return (bool)GetValue(ShowOtherProperty); }
			set { SetValue(ShowOtherProperty, value); }
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

		private static void OnDataSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LevelFilterControl) dependencyObject).OnDataSourceChanged(args.OldValue as DataSourceViewModel,
			                                                            args.NewValue as DataSourceViewModel);
		}

		private void OnDataSourceChanged(DataSourceViewModel oldValue, DataSourceViewModel newValue)
		{
			if (oldValue != null)
			{
				oldValue.PropertyChanged -= DataSourceOnPropertyChanged;
			}
			if (newValue != null)
			{
				newValue.PropertyChanged += DataSourceOnPropertyChanged;
			}
			OnLevelsChanged();
			OnOtherFilterChanged();
		}

		private void DataSourceOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case "LevelsFilter":
					OnLevelsChanged();
					break;

				case "OtherFilter":
					OnOtherFilterChanged();
					break;
			}
		}

		private void OnOtherFilterChanged()
		{
			ShowOther = !DataSource.OtherFilter;
		}

		private static void OnShowOtherChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LevelFilterControl)dependencyObject).OnShowOtherChanged((bool)args.NewValue);
		}

		private void OnShowOtherChanged(bool showOther)
		{
			if (DataSource == null)
				return;

			DataSource.OtherFilter = !showOther;
		}

		private static void OnFatalChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LevelFilterControl) dependencyObject).OnFatalChanged((bool) args.NewValue);
		}

		private void OnFatalChanged(bool isChecked)
		{
			if (DataSource == null)
				return;

			const LevelFlags level = LevelFlags.Fatal;
			if (isChecked)
			{
				DataSource.LevelsFilter |= level;
			}
			else
			{
				DataSource.LevelsFilter &= ~level;
			}
		}

		private static void OnErrorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LevelFilterControl) dependencyObject).OnErrorChanged((bool) args.NewValue);
		}

		private void OnErrorChanged(bool isChecked)
		{
			if (DataSource == null)
				return;

			const LevelFlags level = LevelFlags.Error;
			if (isChecked)
			{
				DataSource.LevelsFilter |= level;
			}
			else
			{
				DataSource.LevelsFilter &= ~level;
			}
		}

		private static void OnWarningChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LevelFilterControl) dependencyObject).OnWarningChanged((bool) args.NewValue);
		}

		private void OnWarningChanged(bool isChecked)
		{
			if (DataSource == null)
				return;

			const LevelFlags level = LevelFlags.Warning;
			if (isChecked)
			{
				DataSource.LevelsFilter |= level;
			}
			else
			{
				DataSource.LevelsFilter &= ~level;
			}
		}

		private static void OnInfoChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LevelFilterControl) dependencyObject).OnInfoChanged((bool) args.NewValue);
		}

		private void OnInfoChanged(bool isChecked)
		{
			if (DataSource == null)
				return;

			const LevelFlags level = LevelFlags.Info;
			if (isChecked)
			{
				DataSource.LevelsFilter |= level;
			}
			else
			{
				DataSource.LevelsFilter &= ~level;
			}
		}

		private static void OnDebugChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LevelFilterControl) dependencyObject).OnDebugChanged((bool) args.NewValue);
		}

		private void OnDebugChanged(bool isChecked)
		{
			if (DataSource == null)
				return;

			const LevelFlags level = LevelFlags.Debug;
			if (isChecked)
			{
				DataSource.LevelsFilter |= level;
			}
			else
			{
				DataSource.LevelsFilter &= ~level;
			}
		}

		private void OnLevelsChanged()
		{
			if (DataSource == null)
				return;

			var levels = DataSource.LevelsFilter;
			ShowFatal = levels.HasFlag(LevelFlags.Fatal);
			ShowError = levels.HasFlag(LevelFlags.Error);
			ShowWarning = levels.HasFlag(LevelFlags.Warning);
			ShowInfo = levels.HasFlag(LevelFlags.Info);
			ShowDebug = levels.HasFlag(LevelFlags.Debug);
		}
	}
}