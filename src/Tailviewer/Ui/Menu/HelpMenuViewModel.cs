using System.Collections.Generic;
using System.Windows.Input;
using Metrolib;

namespace Tailviewer.Ui.Menu
{
	public sealed class HelpMenuViewModel
		: IMenu
	{
		private readonly IMenuViewModel[] _items;

		public HelpMenuViewModel(ICommand reportIssue,
		                         ICommand suggestFeature,
		                         ICommand askQuestion,
		                         ICommand checkForUpdates,
		                         ICommand showLogCommand,
		                         ICommand showAboutFlyout)
		{
			var feedbackMenuItems = new[]
			{
				new CommandMenuViewModel(reportIssue)
				{
					Header = "Report a Problem...",
					Icon = Icons.AlertCircleOutline
				},
				new CommandMenuViewModel(suggestFeature)
				{
					Header = "Suggest a Feature...",
					Icon = Icons.ChatOutline
				},
				new CommandMenuViewModel(askQuestion)
				{
					Header = "Ask a Question...",
					Icon = Icons.ChatQuestionOutline
				}
			};
			_items = new IMenuViewModel[]
			{
				new ParentMenuViewModel(feedbackMenuItems)
				{
					Header = "Send Feedback",
				},
				null,
				new CommandMenuViewModel(checkForUpdates)
				{
					Header = "Check For Updates"
				},
				new CommandMenuViewModel(showLogCommand)
				{
					Header = "Show Log"
				},
				null,
				new CommandMenuViewModel(showAboutFlyout)
				{
					Header = "About"
				}
			};
		}

		#region Implementation of IMenu

		public IEnumerable<IMenuViewModel> Items => _items;

		#endregion
	}
}
