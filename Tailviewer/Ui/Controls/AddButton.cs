using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tailviewer.Ui.Controls
{
	public class AddButton : Button
	{
		private static readonly DependencyPropertyKey IsWhitePropertyKey
			= DependencyProperty.RegisterReadOnly("IsWhite", typeof (bool), typeof (AddButton),
			                                      new FrameworkPropertyMetadata(false,
			                                                                    FrameworkPropertyMetadataOptions.None));

		public static readonly DependencyProperty IsWhiteProperty = IsWhitePropertyKey.DependencyProperty;

		private static readonly DependencyPropertyKey IsBlackPropertyKey
			= DependencyProperty.RegisterReadOnly("IsBlack", typeof (bool), typeof (AddButton),
			                                      new FrameworkPropertyMetadata(true,
			                                                                    FrameworkPropertyMetadataOptions.None));

		public static readonly DependencyProperty IsBlackProperty = IsBlackPropertyKey.DependencyProperty;

		static AddButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (AddButton), new FrameworkPropertyMetadata(typeof (AddButton)));
		}

		public AddButton()
		{
			MouseEnter += OnMouseEnter;
			MouseLeave += OnMouseLeave;
		}

		public bool IsWhite
		{
			get { return (bool) GetValue(IsWhiteProperty); }
			protected set { SetValue(IsWhitePropertyKey, value); }
		}

		public bool IsBlack
		{
			get { return (bool) GetValue(IsBlackProperty); }
			protected set { SetValue(IsBlackPropertyKey, value); }
		}

		private void OnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
		{
			UpdateBlackAndWhite(false, IsPressed);
		}

		private void OnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
		{
			UpdateBlackAndWhite(true, IsPressed);
		}

		protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnIsPressedChanged(e);

			UpdateBlackAndWhite(IsMouseOver, (bool) e.NewValue);
		}

		private void UpdateBlackAndWhite(bool isMouseOver, bool isPressed)
		{
			if (isMouseOver || isPressed)
			{
				IsWhite = true;
				IsBlack = false;
			}
			else
			{
				IsWhite = false;
				IsBlack = true;
			}
		}
	}
}