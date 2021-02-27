using System.Reflection;
using System.Windows;
using System.Windows.Input;
using log4net;
using Metrolib;
using Metrolib.Controls;

namespace Tailviewer.Ui.GoToLine
{
	public sealed class GoToLinePopup
		: AutoPopup<EditorTextBox>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		static GoToLinePopup()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(GoToLinePopup),
			                                         new FrameworkPropertyMetadata(typeof(GoToLinePopup)));
		}

		public GoToLinePopup()
		{
			InputBindings.Add(new InputBinding(new DelegateCommand2(OnEnter), new KeyGesture(Key.Enter)));
		}

		private void OnEnter()
		{
			var viewModel = DataContext as GoToLineViewModel;
			if (viewModel != null)
			{
				viewModel.ChoseLineNumber();
				HidePopup();
			}
			else
			{
				Log.WarnFormat("Expected DataContext '{0}' to be of type '{1}' but it isn't!",
				               DataContext,
				               typeof(GoToLineViewModel));
			}
		}
	}
}