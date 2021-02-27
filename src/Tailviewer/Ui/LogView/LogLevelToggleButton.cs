using System.Windows;
using System.Windows.Media;
using Metrolib;
using Metrolib.Controls;

namespace Tailviewer.Ui.LogView
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

		private static readonly DependencyPropertyKey IconPropertyKey
			= DependencyProperty.RegisterReadOnly("Icon", typeof(Geometry), typeof(LogLevelToggleButton),
			                                      new FrameworkPropertyMetadata(default(Geometry),
				                                                                    FrameworkPropertyMetadataOptions
					                                                                    .None));

		public static readonly DependencyProperty IconProperty
			= IconPropertyKey.DependencyProperty;

		private static readonly Geometry ScriptTextRemoveOutline;
		private static readonly Geometry AlertCircleRemoveOutline;
		private static readonly Geometry AlertRemoveOutline;
		private static readonly Geometry AlertRhombusRemoveOutline;
		private static readonly Geometry BugRemoveOutline;
		private static readonly Geometry ChatRemoveOutline;
		private static readonly Geometry InformationRemoveOutline;

		static LogLevelToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LogLevelToggleButton),
			                                         new FrameworkPropertyMetadata(typeof(LogLevelToggleButton)));

			Geometry CreateGeometry(string data)
			{
				var geometry = Geometry.Parse(data);
				geometry.Freeze();
				return geometry;
			}
			ScriptTextRemoveOutline = CreateGeometry("M 8 2 C 6.3 2 5 3.3 5 5 L 5 16 L 7 16 L 7 5 C 7 4.4 7.4 4 8 4 L 16 4 L 16 13.800781 C 16.6 13.400781 17.3 13.199609 18 13.099609 L 18 5 C 18 4.4 18.4 4 19 4 C 19.6 4 20 4.4 20 5 L 20 6 L 22 6 L 22 5 C 22 3.3 20.7 2 19 2 L 8 2 z M 9 6 L 9 8 L 14 8 L 14 6 L 9 6 z M 9 10 L 9 12 L 14 12 L 14 10 L 9 10 z M 9 14 L 9 16 L 13.800781 16 C 13.900781 15.9 13.9 15.799219 14 15.699219 L 14 14 L 9 14 z M 17.191406 15.425781 L 15.78125 16.845703 L 17.900391 18.964844 L 15.78125 21.085938 L 17.191406 22.505859 L 19.310547 20.375 L 21.429688 22.505859 L 22.849609 21.085938 L 20.720703 18.964844 L 22.849609 16.845703 L 21.429688 15.425781 L 19.310547 17.554688 L 17.191406 15.425781 z M 2 18 L 2 19 C 2 20.7 3.3 22 5 22 L 13.800781 22 C 13.300781 21.1 13 20.1 13 19 L 13 18.400391 L 13 18 L 2 18 z ");
			AlertCircleRemoveOutline = CreateGeometry("M 12 2 C 6.47 2 2 6.5 2 12 A 10 10 0 0 0 12 22 A 10 10 0 0 0 14.537109 21.671875 A 6.1630119 6.1630119 0 0 1 14.001953 19.746094 A 8 8 0 0 1 12 20 A 8 8 0 0 1 4 12 A 8 8 0 0 1 12 4 A 8 8 0 0 1 20 12 A 8 8 0 0 1 19.945312 12.921875 A 6.1630119 6.1630119 0 0 1 20.101562 12.917969 A 6.1630119 6.1630119 0 0 1 20.130859 12.917969 A 6.1630119 6.1630119 0 0 1 21.927734 13.1875 A 10 10 0 0 0 22 12 A 10 10 0 0 0 12 2 z M 11 7 L 11 13 L 13 13 L 13 7 L 11 7 z M 11 15 L 11 17 L 13 17 L 13 15 L 11 15 z M 17.857422 15.449219 L 16.447266 16.869141 L 18.566406 18.990234 L 16.447266 21.109375 L 17.857422 22.529297 L 19.976562 20.398438 L 22.097656 22.529297 L 23.517578 21.109375 L 21.386719 18.990234 L 23.517578 16.869141 L 22.097656 15.449219 L 19.976562 17.580078 L 17.857422 15.449219 z ");
			AlertRemoveOutline = CreateGeometry("M11 15.5H13V17.5H11V15.5M14 19C14 18.86 14 18.73 14 18.6H5.4L12 7.3L16.11 14.44C16.62 14 17.2 13.65 17.84 13.41L12 3.3L2 20.6H14.22C14.08 20.09 14 19.56 14 19M13 10.5H11V14.5H13V10.5M22.12 15.46L20 17.59L17.88 15.46L16.47 16.88L18.59 19L16.47 21.12L17.88 22.54L20 20.41L22.12 22.54L23.54 21.12L21.41 19L23.54 16.88L22.12 15.46Z");
			AlertRhombusRemoveOutline = CreateGeometry("M 12 2 C 11.5 2 10.999844 2.1898437 10.589844 2.5898438 L 2.5898438 10.589844 C 1.7998437 11.369844 1.7998437 12.630156 2.5898438 13.410156 L 10.589844 21.410156 C 11.369844 22.200156 12.630156 22.200156 13.410156 21.410156 L 14.175781 20.644531 A 6.1630119 6.1630119 0 0 1 13.962891 19.050781 A 6.1630119 6.1630119 0 0 1 14.066406 17.933594 L 12 20 L 4 12 L 12 4 L 20 12 L 19.009766 12.990234 A 6.1630119 6.1630119 0 0 1 20.099609 12.888672 A 6.1630119 6.1630119 0 0 1 20.128906 12.888672 A 6.1630119 6.1630119 0 0 1 21.675781 13.085938 C 22.186021 12.312651 22.098116 11.269095 21.410156 10.589844 L 13.410156 2.5898438 C 13.000156 2.1898437 12.5 2 12 2 z M 11 7 L 11 13 L 13 13 L 13 7 L 11 7 z M 11 15 L 11 17 L 13 17 L 13 15 L 11 15 z M 17.855469 15.417969 L 16.445312 16.837891 L 18.564453 18.958984 L 16.445312 21.078125 L 17.855469 22.498047 L 19.974609 20.369141 L 22.095703 22.498047 L 23.515625 21.078125 L 21.384766 18.958984 L 23.515625 16.837891 L 22.095703 15.417969 L 19.974609 17.548828 L 17.855469 15.417969 z ");
			BugRemoveOutline = CreateGeometry("M 8.4101562 3 L 7 4.4101562 L 8.6191406 6 C 7.8691406 6.5 7.2605469 7.21 6.8105469 8 L 4 8 L 4 10 L 6.0898438 10 C 6.0298438 10.33 6 10.66 6 11 L 6 12 L 4 12 L 4 14 L 6 14 L 6 15 C 6 15.34 6.0298438 15.67 6.0898438 16 L 4 16 L 4 18 L 6.8105469 18 C 8.3270231 20.62186 11.518813 21.650924 14.240234 20.544922 A 6.0511514 6.0511514 0 0 1 14.058594 19.082031 A 6.0511514 6.0511514 0 0 1 14.099609 18.404297 A 4 4 0 0 1 12 19 A 4 4 0 0 1 8 15 L 8 11 A 4 4 0 0 1 12 7 A 4 4 0 0 1 16 11 L 16 14.642578 A 6.0511514 6.0511514 0 0 1 20 13.033203 L 20 12 L 18 12 L 18 11 C 18 10.66 17.970156 10.33 17.910156 10 L 20 10 L 20 8 L 17.189453 8 C 16.739453 7.2 16.119141 6.5 15.369141 6 L 17 4.4101562 L 15.589844 3 L 13.419922 5.1699219 C 12.959922 5.0599219 12.5 5 12 5 C 11.5 5 11.049844 5.0599219 10.589844 5.1699219 L 8.4101562 3 z M 10 10 L 10 12 L 14 12 L 14 10 L 10 10 z M 10 14 L 10 16 L 14 16 L 14 14 L 10 14 z M 17.859375 15.460938 L 16.449219 16.880859 L 18.570312 19.001953 L 16.449219 21.121094 L 17.859375 22.541016 L 19.980469 20.412109 L 22.099609 22.541016 L 23.519531 21.121094 L 21.390625 19.001953 L 23.519531 16.880859 L 22.099609 15.460938 L 19.980469 17.591797 L 17.859375 15.460938 z ");
			ChatRemoveOutline = CreateGeometry("M 12 3 C 6.5 3 2 6.58 2 11 C 2.05 13.15 3.06 15.17 4.75 16.5 C 4.75 17.1 4.33 18.67 2 21 C 4.37 20.89 6.6407021 20 8.4707031 18.5 C 9.6107031 18.83 10.81 19 12 19 C 12.67395 19 13.33127 18.944264 13.96875 18.841797 A 6.1630119 6.1630119 0 0 1 14.427734 16.716797 C 13.661639 16.89983 12.847088 17 12 17 C 7.58 17 4 14.31 4 11 C 4 7.69 7.58 5 12 5 C 16.419999 5 20 7.69 20 11 C 20 11.671903 19.846333 12.316428 19.574219 12.919922 A 6.1630119 6.1630119 0 0 1 20.101562 12.894531 A 6.1630119 6.1630119 0 0 1 20.130859 12.894531 A 6.1630119 6.1630119 0 0 1 21.642578 13.083984 C 21.867737 12.418384 22 11.722447 22 11 C 22 6.58 17.499999 3 12 3 z M 17.857422 15.425781 L 16.447266 16.845703 L 18.566406 18.964844 L 16.447266 21.085938 L 17.857422 22.505859 L 19.976562 20.375 L 22.097656 22.505859 L 23.517578 21.085938 L 21.386719 18.964844 L 23.517578 16.845703 L 22.097656 15.425781 L 19.976562 17.554688 L 17.857422 15.425781 z ");
			InformationRemoveOutline = CreateGeometry("M 12 2 A 10 10 0 0 0 2 12 A 10 10 0 0 0 12 22 A 10 10 0 0 0 14.527344 21.673828 A 6.1626956 6.1626956 0 0 1 13.984375 19.740234 C 13.34827 19.903665 12.686257 20 12 20 C 7.5899999 20 4 16.41 4 12 C 4 7.59 7.5899999 4 12 4 C 16.41 4 20 7.59 20 12 C 20 12.307514 19.979292 12.609602 19.945312 12.908203 A 6.1626956 6.1626956 0 0 1 20.082031 12.904297 A 6.1626956 6.1626956 0 0 1 20.111328 12.904297 A 6.1626956 6.1626956 0 0 1 21.929688 13.179688 A 10 10 0 0 0 22 12 A 10 10 0 0 0 12 2 z M 11 7 L 11 9 L 13 9 L 13 7 L 11 7 z M 11 11 L 11 17 L 13 17 L 13 11 L 11 11 z M 17.837891 15.435547 L 16.427734 16.855469 L 18.548828 18.974609 L 16.427734 21.095703 L 17.837891 22.515625 L 19.958984 20.384766 L 22.078125 22.515625 L 23.498047 21.095703 L 21.369141 18.974609 L 23.498047 16.855469 L 22.078125 15.435547 L 19.958984 17.564453 L 17.837891 15.435547 z ");
		}

		public LogLevelToggleButton()
		{
			Checked += OnIsCheckedChanged;
			Unchecked += OnIsCheckedChanged;
		}

		public Geometry Icon
		{
			get { return (Geometry) GetValue(IconProperty); }
			private set { SetValue(IconPropertyKey, value); }
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

		private void OnIsCheckedChanged(object sender, RoutedEventArgs e)
		{
			UpdateIcon(IsChecked, LogLevel);
		}

		private void OnLogLevelChanged(LevelFlags logLevel)
		{
			UpdateIcon(IsChecked, logLevel);
		}

		private void UpdateIcon(bool? isChecked, LevelFlags logLevel)
		{
			Icon = isChecked == true
				? GetCheckedIcon(logLevel)
				: GetUncheckedIcon(logLevel);
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
				case LevelFlags.All: return null;
				default:
					return null;
			}
		}

		private Geometry GetUncheckedIcon(LevelFlags logLevel)
		{
			switch (logLevel)
			{
				case LevelFlags.Other: return null;
				case LevelFlags.Trace: return ChatRemoveOutline;
				case LevelFlags.Debug: return BugRemoveOutline;
				case LevelFlags.Info: return InformationRemoveOutline;
				case LevelFlags.Warning: return AlertRemoveOutline;
				case LevelFlags.Error: return AlertCircleRemoveOutline;
				case LevelFlags.Fatal: return AlertRhombusRemoveOutline;
				case LevelFlags.All: return null;
				default:
					return null;
			}
		}
	}
}