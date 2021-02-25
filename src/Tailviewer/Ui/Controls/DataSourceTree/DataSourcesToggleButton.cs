using System.Windows;
using Metrolib.Controls;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls.DataSourceTree
{
	/// <summary>
	///     The button with which the user toggles the visibility of the <see cref="DataSourcesControl" />.
	/// </summary>
	public sealed class DataSourcesToggleButton
		: FlatToggleButton
	{
		public static readonly DependencyProperty SelectedDataSourceProperty = DependencyProperty.Register(
		                                                "SelectedDataSource", typeof(IDataSourceViewModel), typeof(DataSourcesToggleButton), new PropertyMetadata(default(IDataSourceViewModel)));

		public IDataSourceViewModel SelectedDataSource
		{
			get { return (IDataSourceViewModel) GetValue(SelectedDataSourceProperty); }
			set { SetValue(SelectedDataSourceProperty, value); }
		}

		static DataSourcesToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DataSourcesToggleButton),
			                                         new FrameworkPropertyMetadata(typeof(DataSourcesToggleButton)));
		}
	}
}