using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.Controls.QuickNavigation
{
	/// <summary>
	///     Responsible for giving the user a list of opened data sources which match a search term.
	/// </summary>
	public sealed class QuickNavigationViewModel
		: INotifyPropertyChanged
	{
		private readonly IDataSources _dataSources;
		private readonly ICommand _chooseDataSourceCommand;

		private IReadOnlyList<DataSourceSuggestionViewModel> _suggestions;
		private string _searchTerm;
		private DataSourceSuggestionViewModel _selectedSuggestion;

		public event Action<IDataSource> DataSourceChosen;

		public QuickNavigationViewModel(IDataSources dataSources)
		{
			if (dataSources == null)
				throw new ArgumentNullException(nameof(dataSources));

			_dataSources = dataSources;
			_chooseDataSourceCommand = new DelegateCommand<DataSourceSuggestionViewModel>(OnDataSourceChosen);
		}

		private void OnDataSourceChosen(DataSourceSuggestionViewModel dataSource)
		{
			DataSourceChosen?.Invoke(dataSource.DataSource);
		}

		public string SearchTerm
		{
			get { return _searchTerm; }
			set
			{
				if (value == _searchTerm)
					return;

				_searchTerm = value;
				EmitPropertyChanged();

				if (!string.IsNullOrEmpty(value))
				{
					Suggestions = FindMatches(value);
					if (_suggestions.Count > 0)
					{
						SelectedSuggestion = _suggestions[0];
					}
				}
				else
				{
					Suggestions = null;
				}
			}
		}

		public IReadOnlyList<DataSourceSuggestionViewModel> Suggestions
		{
			get { return _suggestions; }
			private set
			{
				_suggestions = value;
				EmitPropertyChanged();
			}
		}

		public ICommand ChooseDataSourceCommand => _chooseDataSourceCommand;

		public DataSourceSuggestionViewModel SelectedSuggestion
		{
			get { return _selectedSuggestion; }
			set
			{
				if (value == _selectedSuggestion)
					return;

				_selectedSuggestion = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[Pure]
		private IReadOnlyList<DataSourceSuggestionViewModel> FindMatches(string searchPattern)
		{
			return _dataSources.Sources.Select(x => TryCreateMatch(x, searchPattern))
			                   .Where(x => x != null)
			                   .ToList();
		}

		private DataSourceSuggestionViewModel TryCreateMatch(IDataSource dataSource, string searchPattern)
		{
			var fileName = dataSource.FullFileName;
			var idx = fileName.IndexOf(searchPattern, StringComparison.CurrentCultureIgnoreCase);
			if (idx == -1)
				return null;

			var prefix = fileName.Substring(startIndex: 0, length: idx);
			var midfix = fileName.Substring(idx, searchPattern.Length);
			var postfix = fileName.Substring(idx + searchPattern.Length);
			return new DataSourceSuggestionViewModel(dataSource, prefix, midfix, postfix);
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}