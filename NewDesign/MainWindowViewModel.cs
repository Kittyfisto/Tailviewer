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
		private DataSource _selectedTask;

		public MainWindowViewModel()
		{
			Tasks = new List<DataSource>
			{
				new DataSource("Home", "Home"),
				new DataSource("Visualise", "Visualise"),
				new DataSource("Raw", "Raw"),
				new DataSource("Settings", "Settings")
			};
			SelectedTask = Tasks[1];
		}

		public List<DataSource> Tasks { get; }

		public DataSource SelectedTask
		{
			get { return _selectedTask; }
			set
			{
				if (value == _selectedTask)
					return;

				_selectedTask = value;
				EmitPropertyChanged();

				if (value == Tasks[1])
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