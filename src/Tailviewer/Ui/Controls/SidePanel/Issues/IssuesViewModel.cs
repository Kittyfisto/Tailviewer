using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Tailviewer.BusinessLogic.Plugins.Issues;

namespace Tailviewer.Ui.Controls.SidePanel.Issues
{
	/// <summary>
	///     Responsible for presenting a list of <see cref="IssueViewModel" />s based from the
	///     issues reported by a <see cref="ILogFileIssueAnalyser" />.
	/// </summary>
	internal sealed class IssuesViewModel
		: ILogFileIssueListener
	{
		private readonly ObservableCollection<IssueViewModel> _issues;
		private readonly Dictionary<LogFileIssue, IssueViewModel> _viewModelsByIssue;
		private readonly ILogFileIssueAnalyser _analyser;
		private IReadOnlyList<LogFileIssue> _currentIssues;

		public IssuesViewModel(ILogFileIssueAnalyser analyser)
		{
			_issues = new ObservableCollection<IssueViewModel>();
			_viewModelsByIssue = new Dictionary<LogFileIssue, IssueViewModel>();

			_analyser = analyser;
			_analyser.AddListener(this);
			_analyser.Start();
		}

		public void Dispose()
		{
			_analyser.Dispose();
		}

		public IEnumerable<IssueViewModel> Issues => _issues;

		#region Implementation of ILogFileIssueListener

		public void OnIssuesChanged(IEnumerable<LogFileIssue> issues)
		{
			_currentIssues = issues.ToList();
		}

		#endregion

		public void Update()
		{
			var currentIssues = Interlocked.Exchange(ref _currentIssues, value: null);
			if (currentIssues == null)
				return;

			AddNewIssues(currentIssues);
			RemoveOldIssues(currentIssues);
		}

		private void AddNewIssues(IReadOnlyList<LogFileIssue> currentIssues)
		{
			foreach (var issue in currentIssues)
				if (_viewModelsByIssue.ContainsKey(issue))
				{
					var viewModel = new IssueViewModel(issue);
					_viewModelsByIssue.Add(issue, viewModel);
					_issues.Add(viewModel);
				}
		}

		private void RemoveOldIssues(IReadOnlyList<LogFileIssue> currentIssues)
		{
			for (var i = _issues.Count - 1; i >= 0; --i)
			{
				var viewModel = _issues[i];
				if (!currentIssues.Contains(viewModel.Issue))
				{
					_viewModelsByIssue.Remove(viewModel.Issue);
					_issues.RemoveAt(i);
				}
			}
		}
	}
}