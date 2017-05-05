using System.Collections.Generic;
using System.ComponentModel;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public interface IMainPanelViewModel
		: INotifyPropertyChanged
	{
		IDataSourceViewModel CurrentDataSource { get; set; }
		IEnumerable<ILogEntryFilter> QuickFilterChain { get; set; }

		void Update();
	}
}
