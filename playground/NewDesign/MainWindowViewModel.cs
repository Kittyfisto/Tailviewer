using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NewDesign.Dashboard;

namespace NewDesign
{
	public sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		private object _selectedViewModel;
		private MenuItem _selectedTask;

		public MainWindowViewModel()
		{
			Tasks = new List<MenuItem>
			{
				new MenuItem("Home", "Home"),
				new MenuItem("Data", "Data"),
				new MenuItem("Visualise", "Visualise"),
				new MenuItem("Raw", "Raw"),
				new MenuItem("Settings", "Settings")
			};
			SelectedTask = Tasks[2];
		}

		public List<MenuItem> Tasks { get; }

		public MenuItem SelectedTask
		{
			get { return _selectedTask; }
			set
			{
				if (value == _selectedTask)
					return;

				_selectedTask = value;
				EmitPropertyChanged();

				if (value == Tasks[2])
				{
					SelectedViewModel = new DashboardViewModel();
				}
				else
				{
					SelectedViewModel = null;
				}
			}
		}

		public object SelectedViewModel
		{
			get { return _selectedViewModel; }
			set
			{
				if (value == _selectedViewModel)
					return;

				_selectedViewModel = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}