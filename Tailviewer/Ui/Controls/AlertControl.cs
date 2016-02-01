using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tailviewer.Ui.Controls
{
	public class AlertControl : Control
	{
		public static readonly DependencyProperty ExceptionProperty =
			DependencyProperty.Register("Exception", typeof (Exception), typeof (AlertControl),
			                            new PropertyMetadata(default(Exception), OnExceptionChanged));

		public static readonly DependencyProperty ExceptionTypeProperty =
			DependencyProperty.Register("ExceptionType", typeof (Type), typeof (AlertControl),
			                            new PropertyMetadata(default(Type)));

		public static readonly DependencyProperty CloseCommandProperty =
			DependencyProperty.Register("CloseCommand", typeof (ICommand), typeof (AlertControl),
			                            new PropertyMetadata(default(ICommand)));

		static AlertControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (AlertControl), new FrameworkPropertyMetadata(typeof (AlertControl)));
		}

		public ICommand CloseCommand
		{
			get { return (ICommand) GetValue(CloseCommandProperty); }
			set { SetValue(CloseCommandProperty, value); }
		}

		public Type ExceptionType
		{
			get { return (Type) GetValue(ExceptionTypeProperty); }
			set { SetValue(ExceptionTypeProperty, value); }
		}

		public Exception Exception
		{
			get { return (Exception) GetValue(ExceptionProperty); }
			set { SetValue(ExceptionProperty, value); }
		}

		private static void OnExceptionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((AlertControl) dependencyObject).OnExceptionChanged((Exception) args.NewValue);
		}

		private void OnExceptionChanged(Exception exception)
		{
			ExceptionType = exception != null ? exception.GetType() : null;
		}
	}
}