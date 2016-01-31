using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tailviewer.Ui.Controls
{
	public class FilterTextBox : Control
	{
		public static readonly DependencyProperty FilterTextProperty =
			DependencyProperty.Register("FilterText", typeof (string), typeof (FilterTextBox),
			                            new PropertyMetadata(null, OnFilterTextChanged));

		public Button RemoveFilterTextButton
		{
			get { return _removeFilterTextButton; }
		}

		private static void OnFilterTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((FilterTextBox) dependencyObject).OnFilterTextChanged((string) args.NewValue);
		}

		private void OnFilterTextChanged(string filterText)
		{
			HasFilterText = !string.IsNullOrEmpty(filterText);
		}

		public static readonly DependencyProperty WatermarkProperty =
			DependencyProperty.Register("Watermark", typeof (string), typeof (FilterTextBox),
			                            new PropertyMetadata(default(string)));

		public static readonly DependencyProperty IsValidProperty =
			DependencyProperty.Register("IsValid", typeof (bool), typeof (FilterTextBox), new PropertyMetadata(true));

		private static readonly DependencyPropertyKey HasFilterTextPropertyKey
		= DependencyProperty.RegisterReadOnly("HasFilterText", typeof(bool), typeof(FilterTextBox),
			new FrameworkPropertyMetadata(false,
				FrameworkPropertyMetadataOptions.None));

		public static readonly DependencyProperty HasFilterTextProperty
			= HasFilterTextPropertyKey.DependencyProperty;

		public bool HasFilterText
		{
			get { return (bool)GetValue(HasFilterTextProperty); }
			protected set { SetValue(HasFilterTextPropertyKey, value); }
		}

		public bool IsValid
		{
			get { return (bool) GetValue(IsValidProperty); }
			set { SetValue(IsValidProperty, value); }
		}

		private TextBox _filterInput;
		private Button _removeFilterTextButton;

		static FilterTextBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (FilterTextBox),
			                                         new FrameworkPropertyMetadata(typeof (FilterTextBox)));
		}

		public FilterTextBox()
		{
			GotFocus += OnGotFocus;
		}

		public string Watermark
		{
			get { return (string) GetValue(WatermarkProperty); }
			set { SetValue(WatermarkProperty, value); }
		}

		public string FilterText
		{
			get { return (string) GetValue(FilterTextProperty); }
			set { SetValue(FilterTextProperty, value); }
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				var scope = FocusManager.GetFocusScope(_filterInput);
				FocusManager.SetFocusedElement(scope, null);
				Application.Current.MainWindow.Focus();
			}
			base.OnKeyDown(e);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_removeFilterTextButton != null)
			{
				_removeFilterTextButton.Click -= RemoveFilterTextButtonOnClick;
			}

			_filterInput = (TextBox) GetTemplateChild("PART_FilterInput");
			_removeFilterTextButton = (Button) GetTemplateChild("PART_RemoveFilterText");

			if (_removeFilterTextButton != null)
			{
				_removeFilterTextButton.Click += RemoveFilterTextButtonOnClick;
			}
		}

		private void RemoveFilterTextButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
		{
			FilterText = null;
		}

		private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
		{
			_filterInput.Focus();
		}
	}
}