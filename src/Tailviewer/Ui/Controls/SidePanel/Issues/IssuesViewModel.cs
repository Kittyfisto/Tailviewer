using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Plugins.Issues;

namespace Tailviewer.Ui.Controls.SidePanel.Issues
{
	/// <summary>
	///     Responsible for presenting a list of <see cref="IssueViewModel" />s based from the
	///     issues reported by a <see cref="ILogFileIssueAnalyser" />.
	/// </summary>
	internal sealed class IssuesViewModel
		: ILogFileIssueListener
		, INotifyPropertyChanged
	{
		private readonly ILogFileIssueAnalyser _analyser;
		private readonly INavigationService _navigationService;
		private readonly Dictionary<ILogFileIssue, IssueViewModel> _viewModelsByIssue;
		private readonly List<IssueViewModel> _allIssues;

		private IReadOnlyList<ILogFileIssue> _currentIssues;
		private ObservableCollection<IssueViewModel> _filteredIssues;

		private int _criticalCount;
		private int _majorCount;
		private int _minorCount;
		private bool _showCritical;
		private bool _showMajor;
		private bool _showMinor;

		public IssuesViewModel(ILogFileIssueAnalyser analyser,
		                       INavigationService navigationService)
		{
			_allIssues = new List<IssueViewModel>();
			_filteredIssues = new ObservableCollection<IssueViewModel>();
			_viewModelsByIssue = new Dictionary<ILogFileIssue, IssueViewModel>();

			_showCritical = true;
			_showMajor = true;
			_showMinor = true;

			_analyser = analyser;
			_navigationService = navigationService;
			_analyser.AddListener(this);
			_analyser.Start();
		}

		public IReadOnlyList<IssueViewModel> Issues
		{
			get { return _filteredIssues; }
			private set
			{
				_filteredIssues = (ObservableCollection<IssueViewModel>) value;
				EmitPropertyChanged();
			}
		}

		public int Count
		{
			get
			{
				var count = _currentIssues?.Count ?? 0;
				return Math.Max(count, _viewModelsByIssue.Count);
			}
		}

		public int CriticalCount
		{
			get { return _criticalCount; }
			private set
			{
				if (value == _criticalCount)
					return;

				_criticalCount = value;
				EmitPropertyChanged();
			}
		}

		public int MajorCount
		{
			get { return _majorCount; }
			private set
			{
				if (value == _majorCount)
					return;

				_majorCount = value;
				EmitPropertyChanged();
			}
		}

		public int MinorCount
		{
			get { return _minorCount; }
			private set
			{
				if (value == _minorCount)
					return;

				_minorCount = value;
				EmitPropertyChanged();
			}
		}

		public bool ShowCritical
		{
			get { return _showCritical; }
			set
			{
				if (value == _showCritical)
					return;

				_showCritical = value;
				EmitPropertyChanged();
				UpdateFilter();
			}
		}

		public bool ShowMajor
		{
			get { return _showMajor; }
			set
			{
				if (value == _showMajor)
					return;

				_showMajor = value;
				EmitPropertyChanged();
				UpdateFilter();
			}
		}

		public bool ShowMinor
		{
			get { return _showMinor; }
			set
			{
				if (value == _showMinor)
					return;

				_showMinor = value;
				EmitPropertyChanged();
				UpdateFilter();
			}
		}

		#region Implementation of ILogFileIssueListener

		public void OnIssuesChanged(IEnumerable<ILogFileIssue> issues)
		{
			_currentIssues = issues.ToList();
		}

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		public void Dispose()
		{
			_analyser.Dispose();
		}

		public void Update()
		{
			var currentIssues = Interlocked.Exchange(ref _currentIssues, value: null);
			if (currentIssues == null)
				return;

			AddNewIssues(currentIssues);
			RemoveOldIssues(currentIssues);

			var critical = 0;
			var major = 0;
			var minor = 0;
			foreach (var issue in _allIssues)
			{
				switch (issue.Severity)
				{
					case Severity.Critical:
						++critical;
						break;
					case Severity.Major:
						++major;
						break;
					case Severity.Minor:
						++minor;
						break;
				}
			}

			CriticalCount = critical;
			MajorCount = major;
			MinorCount = minor;
		}

		private void AddNewIssues(IReadOnlyList<ILogFileIssue> currentIssues)
		{
			foreach (var issue in currentIssues)
				if (!_viewModelsByIssue.ContainsKey(issue))
				{
					var viewModel = new IssueViewModel(issue, GoToIssue);
					_viewModelsByIssue.Add(issue, viewModel);
					_allIssues.Add(viewModel);
					if (MatchesFilter(viewModel))
						_filteredIssues.Add(viewModel);
				}
		}

		private void GoToIssue(LogLineIndex line)
		{
			// TODO: Maybe we should display a little message to show the user why nothing happens.
			//       For example we might tell the user that that line is filtered out...
			_navigationService.NavigateTo(line);
		}

		private void RemoveOldIssues(IReadOnlyList<ILogFileIssue> currentIssues)
		{
			for (var i = _allIssues.Count - 1; i >= 0; --i)
			{
				var viewModel = _allIssues[i];
				if (!currentIssues.Contains(viewModel.Issue))
				{
					_viewModelsByIssue.Remove(viewModel.Issue);
					_allIssues.RemoveAt(i);
					_filteredIssues.Remove(viewModel);
				}
			}
		}

		private void UpdateFilter()
		{
			Issues = new ObservableCollection<IssueViewModel>(_allIssues.Where(MatchesFilter));
		}

		[Pure]
		private bool MatchesFilter(IssueViewModel issue)
		{
			switch (issue.Severity)
			{
				case Severity.Critical:
					return _showCritical;

				case Severity.Major:
					return _showMajor;

				case Severity.Minor:
					return _showMinor;

				default:
					return false;
			}
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}