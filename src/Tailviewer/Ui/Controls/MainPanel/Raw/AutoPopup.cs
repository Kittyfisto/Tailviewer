using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using log4net;
using Metrolib;
using Tailviewer.Ui.Controls.LogView;

namespace Tailviewer.Ui.Controls.MainPanel.Raw
{
	/// <summary>
	///     A popup which automatically closes itself when its most important
	///     child looses focus or when escape is pressed.
	/// </summary>
	/// <typeparam name="T">The type of the child which shall be focused when opened</typeparam>
	public abstract class AutoPopup<T>
		: Popup
		where T : FrameworkElement
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private T _specialChild;

		protected AutoPopup()
		{
			InputBindings.Add(new KeyBinding(new DelegateCommand2(OnEscape), new KeyGesture(Key.Escape)));

			Loaded += OnLoaded;
			Opened += OnOpened;
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			var children = Child.FindChildrenOfType<T>();
			_specialChild = children.FirstOrDefault();
			if (_specialChild != null)
				_specialChild.IsKeyboardFocusWithinChanged += ChildOnIsKeyboardFocusWithinChanged;
			else
				Log.ErrorFormat("Unable to find textbox: Child is of type '{0}'", Child?.GetType());
		}

		private void OnEscape()
		{
			HidePopup();
		}

		private void ChildOnIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!_specialChild.IsKeyboardFocusWithin)
				HidePopup();
		}

		protected void HidePopup()
		{
			Log.Debug("Hiding popup...");

			IsOpen = false;

			// We also have to focus something else again as
			// the control which previously had focus will be hidden again.
			var logViewer = Application.Current?.MainWindow?.FindChildrenOfType<TextCanvas>().FirstOrDefault();
			if (logViewer != null)
			{
				Log.DebugFormat("Focusing log entry list view again");
				Keyboard.Focus(logViewer);
				logViewer.Focus();
			}
			else
			{
				Log.WarnFormat("Unable to find log entry list view to focus it, focusing main window instead...");
				Application.Current?.MainWindow?.Focus();
			}

			Log.Debug("Popup hidden");
		}

		private void OnOpened(object sender, EventArgs eventArgs)
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				if (_specialChild != null)
					_specialChild.Focus();
				else
					Log.Warn("Can't focus quick navigation control, wrong child...");
			}));
		}
	}
}