using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SharpTail.Ui.Controls
{
	public class RemoveButton : Button
	{
		public static readonly DependencyProperty IsInvertedProperty =
			DependencyProperty.Register("IsInverted", typeof (bool), typeof (RemoveButton),
			                            new PropertyMetadata(false, OnIsInvertedChanged));

		private static readonly DependencyPropertyKey IsWhitePropertyKey
			= DependencyProperty.RegisterReadOnly("IsWhite", typeof (bool), typeof (RemoveButton),
			                                      new FrameworkPropertyMetadata(false,
			                                                                    FrameworkPropertyMetadataOptions.None));

		public static readonly DependencyProperty IsWhiteProperty = IsWhitePropertyKey.DependencyProperty;

		private static readonly DependencyPropertyKey IsBlackPropertyKey
			= DependencyProperty.RegisterReadOnly("IsBlack", typeof(bool), typeof(RemoveButton),
												  new FrameworkPropertyMetadata(true,
																				FrameworkPropertyMetadataOptions.None));

		public static readonly DependencyProperty IsBlackProperty = IsBlackPropertyKey.DependencyProperty;

		static RemoveButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (RemoveButton), new FrameworkPropertyMetadata(typeof (RemoveButton)));
		}

		public bool IsWhite
		{
			get { return (bool)GetValue(IsWhiteProperty); }
			protected set { SetValue(IsWhitePropertyKey, value); }
		}

		public bool IsBlack
		{
			get { return (bool)GetValue(IsBlackProperty); }
			protected set { SetValue(IsBlackPropertyKey, value); }
		}

		public bool IsInverted
		{
			get { return (bool) GetValue(IsInvertedProperty); }
			set { SetValue(IsInvertedProperty, value); }
		}

		private static void OnIsInvertedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((RemoveButton) dependencyObject).OnIsInvertedChanged((bool) args.NewValue);
		}

		public RemoveButton()
		{
			MouseEnter += OnMouseEnter;
			MouseLeave += OnMouseLeave;
		}

		private void OnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
		{
			UpdateBlackAndWhite(IsInverted, false, IsPressed);
		}

		private void OnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
		{
			UpdateBlackAndWhite(IsInverted, true, IsPressed);
		}

		protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnIsPressedChanged(e);

			UpdateBlackAndWhite(IsInverted, IsMouseOver, (bool) e.NewValue);
		}

		private void OnIsInvertedChanged(bool isInverted)
		{
			UpdateBlackAndWhite(isInverted, IsMouseOver, IsPressed);
		}

		private void UpdateBlackAndWhite(bool isInverted, bool isMouseOver, bool isPressed)
		{
			if (isInverted || isMouseOver || isPressed)
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