using System.Windows;
using System.Windows.Controls;
using Tailviewer.Settings;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	///     This toggle button allows the user to chose which display mode is used to describe the source
	///     of a log line. Currently this can either the data source's filename or a character code.
	/// </summary>
	public sealed class DataSourceDisplayModeToggleButton
		: Control
	{
		public static readonly DependencyProperty ShowFilenameProperty = DependencyProperty.Register(
			"ShowFilename", typeof(bool), typeof(DataSourceDisplayModeToggleButton),
			new PropertyMetadata(defaultValue: true, propertyChangedCallback: OnShowFilenameChanged));

		public static readonly DependencyProperty ShowCharacterCodeProperty = DependencyProperty.Register(
			"ShowCharacterCode", typeof(bool), typeof(DataSourceDisplayModeToggleButton),
			new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnShowCharacterCodeChanged));

		public static readonly DependencyProperty DisplayModeProperty = DependencyProperty.Register(
			"DisplayMode", typeof(DataSourceDisplayMode), typeof(DataSourceDisplayModeToggleButton),
			new PropertyMetadata(DataSourceDisplayMode.Filename, OnDisplayModeChanged));

		static DataSourceDisplayModeToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DataSourceDisplayModeToggleButton),
				new FrameworkPropertyMetadata(typeof(DataSourceDisplayModeToggleButton)));
		}

		public bool ShowFilename
		{
			get { return (bool) GetValue(ShowFilenameProperty); }
			set { SetValue(ShowFilenameProperty, value); }
		}

		public bool ShowCharacterCode
		{
			get { return (bool) GetValue(ShowCharacterCodeProperty); }
			set { SetValue(ShowCharacterCodeProperty, value); }
		}

		public DataSourceDisplayMode DisplayMode
		{
			get { return (DataSourceDisplayMode) GetValue(DisplayModeProperty); }
			set { SetValue(DisplayModeProperty, value); }
		}

		private static void OnShowFilenameChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((DataSourceDisplayModeToggleButton) dependencyObject).OnShowFilenameChanged((bool) args.NewValue);
		}

		private void OnShowFilenameChanged(bool showFilename)
		{
			if (showFilename)
				DisplayMode = DataSourceDisplayMode.Filename;
		}

		private static void OnShowCharacterCodeChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs args)
		{
			((DataSourceDisplayModeToggleButton) dependencyObject).OnShowCharacterCodeChanged((bool) args.NewValue);
		}

		private void OnShowCharacterCodeChanged(bool showCharacterCode)
		{
			if (showCharacterCode)
				DisplayMode = DataSourceDisplayMode.CharacterCode;
		}

		private static void OnDisplayModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((DataSourceDisplayModeToggleButton) dependencyObject).OnDisplayModeChanged((DataSourceDisplayMode) args.NewValue);
		}

		private void OnDisplayModeChanged(DataSourceDisplayMode displayMode)
		{
			ShowFilename = displayMode == DataSourceDisplayMode.Filename;
			ShowCharacterCode = displayMode == DataSourceDisplayMode.CharacterCode;
		}
	}
}