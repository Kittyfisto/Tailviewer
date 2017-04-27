using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NewDesign.Dashboard.Layout;
using NewDesign.Dashboard.Widgets;
using NewDesign.Dashboard.Widgets.LineCount;
using NewDesign.Dashboard.Widgets.QuickInfos;

namespace NewDesign.Dashboard
{
	public sealed class DashboardViewModel
		: INotifyPropertyChanged
	{
		private readonly ObservableCollection<WidgetLayoutViewModel> _layouts;
		private readonly ObservableCollection<ISidePanelViewModel> _sidePanels;

		public DashboardViewModel()
		{
			_layouts = new ObservableCollection<WidgetLayoutViewModel>
			{
				new HorizontalWidgetLayoutViewModel
				{
					Widgets =
					{
						new LineCountViewModel
						{
							Count = 1120121,
							Caption = "Log Entries"
						},
						new QuickInfosViewModel(
							new QuickInfoViewModel("Version", new Version(2,0,0)),
							new QuickInfoViewModel("Build", 12351)
							)
						{
							
						}
					}
				}
			};

			_sidePanels = new ObservableCollection<ISidePanelViewModel>
			{
				new WidgetSidePanelViewModel()
			};
		}

		public IEnumerable<WidgetLayoutViewModel> Layouts => _layouts;
		public IEnumerable<ISidePanelViewModel> SidePanels => _sidePanels;

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}