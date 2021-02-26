using System;
using System.Windows.Input;
using System.Windows.Media;

namespace Tailviewer.Ui.Plugins
{
	public interface IPluginViewModel
	{
		Version Version { get; }
		string Author { get; }
		string Name { get; }
		string Description { get; }
		string Error { get; }
		bool HasError { get; }
		Uri Website { get; }
		ImageSource Icon { get; }
		ICommand DeleteCommand { get; }
		ICommand DownloadCommand { get; }
	}
}