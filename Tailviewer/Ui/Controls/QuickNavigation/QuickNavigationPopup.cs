using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using log4net;
using Metrolib;
using Metrolib.Controls;

namespace Tailviewer.Ui.Controls.QuickNavigation
{
	/// <summary>
	///     The popup which hosts a fancy text-box which displays a list of data sources
	///     which match the entered term.
	/// </summary>
	public sealed class QuickNavigationPopup
		: Popup
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private SuggestionInputControl _suggestionsControl;

		static QuickNavigationPopup()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(QuickNavigationPopup),
			                                         new FrameworkPropertyMetadata(typeof(QuickNavigationPopup)));
		}

		public QuickNavigationPopup()
		{
			InputBindings.Add(new KeyBinding(new DelegateCommand2(OnEscape), new KeyGesture(Key.Escape)));

			Loaded += OnLoaded;
			Opened += OnOpened;
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			var children = Child.FindChildrenOfType<SuggestionInputControl>();
			_suggestionsControl = children.FirstOrDefault();
			if (_suggestionsControl != null)
				_suggestionsControl.IsKeyboardFocusWithinChanged += ChildOnIsKeyboardFocusWithinChanged;
			else
				Log.ErrorFormat("Unable to find suggestions control: Child is of type '{0}'", Child?.GetType());
		}

		private void OnEscape()
		{
			HidePopup();
		}

		private void ChildOnIsKeyboardFocusWithinChanged(object sender,
		                                                 DependencyPropertyChangedEventArgs
			                                                 dependencyPropertyChangedEventArgs)
		{
			if (!_suggestionsControl.IsKeyboardFocusWithin)
			{
				HidePopup();
			}
		}

		private void HidePopup()
		{
			IsOpen = false;
			// We also have to focus something else again as
			// the control which previously had focus will be hidden again.
			Application.Current.MainWindow?.Focus();
		}

		private void OnOpened(object sender, EventArgs eventArgs)
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				if (_suggestionsControl != null)
					_suggestionsControl.Focus();
				else
					Log.Warn("Can't focus quick navigation control, wrong child...");
			}));
		}
	}
}