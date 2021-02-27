using System.ComponentModel;
using Tailviewer.Ui.DataSourceTree;
using Tailviewer.Ui.LogView;
using Tailviewer.Ui.Menu;

namespace Tailviewer.Ui
{
	public interface IMainWindowViewModel
		: INotifyPropertyChanged
	{
		IObservableCollection<KeyBindingCommand> KeyBindings { get; }
		LogViewMainPanelViewModel LogViewPanel { get; }
		IFlyoutViewModel CurrentFlyout { get; set; }
		IDataSourceViewModel AddFileOrDirectory(string dataSourceUri);
	}
}