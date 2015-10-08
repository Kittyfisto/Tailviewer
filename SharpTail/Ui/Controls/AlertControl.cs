using System.Windows;
using System.Windows.Controls;

namespace SharpTail.Ui.Controls
{
	public class AlertControl : Control
	{
		public static readonly DependencyProperty ErrorMessageProperty =
			DependencyProperty.Register("ErrorMessage", typeof (string), typeof (AlertControl), new PropertyMetadata(default(string)));

		public string ErrorMessage
		{
			get { return (string) GetValue(ErrorMessageProperty); }
			set { SetValue(ErrorMessageProperty, value); }
		}

		static AlertControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (AlertControl), new FrameworkPropertyMetadata(typeof (AlertControl)));
		}
	}
}