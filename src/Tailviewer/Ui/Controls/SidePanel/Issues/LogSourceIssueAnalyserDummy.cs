using Tailviewer.Plugins;

namespace Tailviewer.Ui.Controls.SidePanel.Issues
{
	internal sealed class LogSourceIssueAnalyserDummy : ILogSourceIssueAnalyser
	{
		#region IDisposable

		public void Dispose()
		{
		}

		#endregion

		#region Implementation of ILogFileIssueAnalyser

		public void AddListener(ILogFileIssueListener listener)
		{
		}

		public void RemoveListener(ILogFileIssueListener listener)
		{
		}

		public void Start()
		{
		}

		#endregion
	}
}