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
			VisualStateManager.GoToState(this, IsMouseOver ? "MouseOverState" : "NormalState", useTransition);
		}
	}
}