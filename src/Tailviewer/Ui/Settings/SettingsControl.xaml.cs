using System.ComponentModel;
using System.Windows;

namespace Tailviewer.Ui.Settings
{
	/// <summary>
	///     Interaction logic for SettingsControl.xaml
	/// </summary>
	public partial class SettingsControl
	{
		public SettingsControl()
		{
			InitializeComponent();

			DataContextChanged += OnDataContextChanged;
		}

		public FrameworkElement ProxyPasswordBox => PART_ProxyPassword;

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			var old = args.OldValue as SettingsFlyoutViewModel;
			if (old != null)
			{
				old.PropertyChanged -= DataContextOnPropertyChanged;
				PART_ProxyPassword.Password = null;
			}

			var @new = args.NewValue as SettingsFlyoutViewModel;
			if (@new != null)
			{
				@new.PropertyChanged += DataContextOnPropertyChanged;
				PART_ProxyPassword.Password = @new.ProxyPassword;
			}
		}

		private void DataContextOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case "ProxyPassword":
					PART_ProxyPassword.Password = ((SettingsFlyoutViewModel) DataContext).ProxyPassword;
					break;
			}
		}

		private void OnPasswordChanged(object sender, RoutedEventArgs e)
		{
			var viewModel = DataContext as SettingsFlyoutViewModel;
			if (viewModel != null)
			{
				viewModel.ProxyPassword = PART_ProxyPassword.Password;
			}
		}
	}
}