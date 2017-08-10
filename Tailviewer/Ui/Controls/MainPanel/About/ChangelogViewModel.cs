using System.Collections.Generic;
using System.Linq;
using Metrolib;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.Ui.Controls.ActionCenter;

namespace Tailviewer.Ui.Controls.MainPanel.About
{
	public sealed class ChangelogViewModel
	{
		private readonly List<ChangeViewModel> _changes;

		public ChangelogViewModel()
		{
			_changes = new List<ChangeViewModel>(Changelog.Changes.Select(CreateViewModel));
			_changes.Reverse();
		}

		private ChangeViewModel CreateViewModel(Change change)
		{
			return new ChangeViewModel(change)
				{
					Title = change.Version.Format()
				};
		}

		public IEnumerable<ChangeViewModel> Changes { get { return _changes; } }
	}
}