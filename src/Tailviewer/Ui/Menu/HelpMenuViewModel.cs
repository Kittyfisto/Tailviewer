using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;

using Tailviewer.Localization;

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
					Header = Strings.MenuReportProblem,
					Icon = Icons.AlertCircleOutline
				},
				new CommandMenuViewModel(suggestFeature)
				{
					Header = Strings.MenuSuggestFeature,
					Icon = Icons.ChatOutline
				},
				new CommandMenuViewModel(askQuestion)
				{
					Header = Strings.MenuAskQuestion,
					Icon = Icons.ChatQuestionOutline
				}
			};
			_items = new IMenuViewModel[]
			{
				new ParentMenuViewModel(feedbackMenuItems)
				{
					Header = Strings.MenuSendFeedback,
				},
				null,
				new CommandMenuViewModel(checkForUpdates)
				{
					Header = Strings.MenuCheckForUpdates
				},
				new CommandMenuViewModel(showLogCommand)
				{
					Header = Strings.MenuShowLog
				},
				null,
				new CommandMenuViewModel(showAboutFlyout)
				{
					Header = Strings.MenuAbout
				}
			};
		}

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#region Implementation of IMenu

		public IEnumerable<IMenuViewModel> Items
		{
			get { return _items; }
		}

		public bool HasItems
		{
			get { return true; }
		}

		#endregion
	}
}