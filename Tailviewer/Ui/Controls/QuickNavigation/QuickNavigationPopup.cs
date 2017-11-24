using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using log4net;
using Metrolib.Controls;

namespace Tailviewer.Ui.Controls.QuickNavigation
{
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
			Loaded += OnLoaded;
			Opened += OnOpened;
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			_suggestionsControl = Child as SuggestionInputControl;
			if (_suggestionsControl != null)
			{
				_suggestionsControl.IsKeyboardFocusWithinChanged += ChildOnIsKeyboardFocusWithinChanged;
			}
			else
			{
				Log.ErrorFormat("Unable to find suggestions control: Child is of type '{0}'", Child?.GetType());
			}
		}

		private void ChildOnIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			if (!_suggestionsControl.IsKeyboardFocusWithin)
			{
				IsOpen = false;
			}
		}

		private void OnOpened(object sender, EventArgs eventArgs)
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				var child = Child as SuggestionInputControl;
				if (child != null)
				{
					child.Focus();
				}
				else
				{
					Log.Warn("Can't focus quick navigation control, wrong child...");
				}
			}));
		}
	}
}
