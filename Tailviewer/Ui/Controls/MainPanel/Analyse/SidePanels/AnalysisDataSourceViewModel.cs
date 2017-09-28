using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels
{
	/// <summary>
	///     Represents a data source in the "data selection" side panel (which is part of the analyse feature).
	///     A user can interact with this data source by chosing if it shall be part of the currently selected analysis
	///     (or not).
	/// </summary>
	public sealed class AnalysisDataSourceViewModel
		: INotifyPropertyChanged
	{
		private readonly IDataSource _dataSource;
		private readonly string _displayName;
		private readonly string _folder;

		private IAnalysisViewModel _currentAnalysis;
		private bool _isSelected;

		public AnalysisDataSourceViewModel(IDataSource dataSource)
		{
			_dataSource = dataSource;
			_displayName = Path.GetFileName(dataSource.FullFileName);
			_folder = Path.GetDirectoryName(dataSource.FullFileName);
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value == _isSelected)
					return;

				_isSelected = value;
				EmitPropertyChanged();

				if (_currentAnalysis != null)
				{
					if (value)
					{
						_dataSource.EnableAnalysis(_currentAnalysis.Id);
						_currentAnalysis.Add(_dataSource.UnfilteredLogFile);
					}
					else
					{
						_dataSource.DisableAnalysis(_currentAnalysis.Id);
						_currentAnalysis.Remove(_dataSource.UnfilteredLogFile);
					}
				}
			}
		}

		public string DisplayName => _displayName;

		public DataSourceId Id => _dataSource.Id;

		public string Folder => _folder;

		public IAnalysisViewModel CurrentAnalysis
		{
			get { return _currentAnalysis; }
			set
			{
				if (value == _currentAnalysis)
					return;

				_currentAnalysis = value;
				EmitPropertyChanged();

				if (value != null)
				{
					IsSelected = _dataSource.IsAnalysisActive(value.Id);
				}
				else
				{
					IsSelected = false;
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Update()
		{
		}
	}
}