using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NewDesign.Dashboard.Layout;
using NewDesign.Dashboard.Widgets.LineCount;
using NewDesign.Dashboard.Widgets.QuickInfo;

namespace NewDesign.Dashboard
{
	public sealed class DashboardViewModel
		: INotifyPropertyChanged
	{
		private readonly ObservableCollection<WidgetLayoutViewModel> _layouts;

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
						new QuickInfoViewModel(
							new NamedValueViewModel("Version", new Version(2,0,0)),
							new NamedValueViewModel("Build", 12351)
							)
						{
							
						}
					}
				}
			};
		}

		public IEnumerable<WidgetLayoutViewModel> Layouts => _layouts;

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}