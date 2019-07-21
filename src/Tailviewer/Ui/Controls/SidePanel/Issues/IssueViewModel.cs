using System;
using System.Windows.Media;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Plugins.Issues;

namespace Tailviewer.Ui.Controls.SidePanel.Issues
{
	internal sealed class IssueViewModel
	{
		private readonly LogFileIssue _issue;

		public IssueViewModel(LogFileIssue issue)
		{
			_issue = issue;
		}

		public Geometry Icon => Icons.Alert;
		public LogLineIndex Line => _issue.Line;
		public string Summary => _issue.Summary;
		public DateTime Timestamp => _issue.Timestamp;
		public Severity Severity => _issue.Severity;

		public LogFileIssue Issue => _issue;
	}
}