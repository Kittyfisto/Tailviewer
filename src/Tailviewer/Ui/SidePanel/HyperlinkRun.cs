using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Tailviewer.Ui.SidePanel
{
	public class LinkRun
		: Run
	{
		public static readonly DependencyProperty NavigateCommandProperty = DependencyProperty.Register(
			"NavigateCommand", typeof(ICommand), typeof(LinkRun), new PropertyMetadata(default(ICommand)));

		public ICommand NavigateCommand
		{
			get { return (ICommand) GetValue(NavigateCommandProperty); }
			set { SetValue(NavigateCommandProperty, value); }
		}

		private static readonly DependencyPropertyKey IsPressedPropertyKey
			= DependencyProperty.RegisterReadOnly("IsPressed", typeof(bool), typeof(LinkRun),
				new FrameworkPropertyMetadata(false,
					FrameworkPropertyMetadataOptions.None,
					OnIsPressedChanged));

		/// <summary>
		///     Definition of the <see cref="IsPressed" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsPressedProperty
			= IsPressedPropertyKey.DependencyProperty;

		/// <summary>
		/// Initializes this run.
		/// </summary>
		public LinkRun()
		{
			MouseLeftButtonUp += OnMouseLeftButtonUp;
			MouseLeftButtonDown += OnMouseLeftButtonDown;
			TouchDown += OnTouchDown;
			TouchUp += OnTouchUp;
			MouseEnter += OnMouseEnter;
			MouseLeave += OnMouseLeave;
			IsEnabledChanged += OnIsEnabledChanged;

			UpdateForeground();
		}
		
		/// <summary>
		///     Whether or not this control is currently being pressed by the left mouse button
		///     or a touch gesture.
		/// </summary>
		public bool IsPressed
		{
			get { return (bool)GetValue(IsPressedProperty); }
			protected set { SetValue(IsPressedPropertyKey, value); }
		}

		private static void OnIsPressedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LinkRun)dependencyObject).OnIsPressedChanged((bool)args.NewValue);
		}

		private void OnIsPressedChanged(bool isPressed)
		{
			UpdateForeground();
		}

		private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			IsPressed = true;
			CaptureMouse();
		}

		private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (IsMouseCaptured)
			{
				ReleaseMouseCapture();
			}

			Navigate();
			e.Handled = true;
		}

		private void OnTouchDown(object sender, TouchEventArgs e)
		{
			IsPressed = true;
		}

		private void OnTouchUp(object sender, TouchEventArgs touchEventArgs)
		{
			IsPressed = false;
			Navigate();
		}

		private void OnMouseEnter(object sender, MouseEventArgs e)
		{
			Cursor = Cursors.Hand;
			UpdateForeground();
		}

		private void OnMouseLeave(object sender, MouseEventArgs e)
		{
			Cursor = null;
			UpdateForeground();
		}

		private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			UpdateForeground();
		}

		private void Navigate()
		{
			var command = NavigateCommand;
			if (command != null)
			{
				if (command.CanExecute(null))
				{
					command.Execute(null);
				}
			}
		}

		private void UpdateForeground()
		{
			if (!IsEnabled)
			{
				Foreground = Metrolib.Constants.ForegroundBrushDisabled;
			}
			else if (IsPressed)
			{
				Foreground = Metrolib.Constants.ForegroundBrushPressed;
			}
			else if (IsMouseOver)
			{
				Foreground = Metrolib.Constants.ForegroundBrushHovered;
			}
			else
			{
				Foreground = Metrolib.Constants.ForegroundBrushAccent;
			}
		}
	}
}
