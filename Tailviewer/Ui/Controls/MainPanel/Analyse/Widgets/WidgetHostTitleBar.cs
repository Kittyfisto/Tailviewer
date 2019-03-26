using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets
{
	public class WidgetHostTitleBar
		: Control
	{
		static WidgetHostTitleBar()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(WidgetHostTitleBar),
			                                         new FrameworkPropertyMetadata(typeof(WidgetHostTitleBar)));
		}

		public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(
		                                                "IsEditing", typeof(bool), typeof(WidgetHostTitleBar),
		                                                new PropertyMetadata(false, OnIsEditingChanged));

		private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((WidgetHostTitleBar) d).UpdateVisualState();
		}

		public bool IsEditing
		{
			get { return (bool) GetValue(IsEditingProperty); }
			set { SetValue(IsEditingProperty, value); }
		}

		public WidgetHostTitleBar()
		{
			MouseEnter += OnMouseEnter;
			MouseLeave += OnMouseLeave;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			UpdateVisualState(false);
		}

		private void OnMouseEnter(object sender, MouseEventArgs e)
		{
			UpdateVisualState();
		}

		private void OnMouseLeave(object sender, MouseEventArgs e)
		{
			UpdateVisualState();
		}

		private void UpdateVisualState(bool useTransition = true)
		{
			VisualStateManager.GoToState(this, IsMouseOver | IsEditing ? "MouseOverState" : "NormalState", useTransition);
		}
	}
}