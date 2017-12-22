using System.Windows;
using System.Windows.Controls;
using Tailviewer.Core.Settings;

namespace Tailviewer.Ui.Controls
{
	public class QuickFilterActionTypeToggleButton : Control
	{
		public static readonly DependencyProperty FilterActionTypeProperty =
			DependencyProperty.Register("FilterActionType", typeof (QuickFilterActionType),
			                            typeof (QuickFilterActionTypeToggleButton),
			                            new PropertyMetadata(default(QuickFilterActionType)));

		public static readonly DependencyProperty IncludeProperty =
			DependencyProperty.Register("Include", typeof (bool), typeof (QuickFilterActionTypeToggleButton),
			                            new PropertyMetadata(default(bool)));

		public static readonly DependencyProperty ExcludeProperty =
			DependencyProperty.Register("Exclude", typeof (bool), typeof (QuickFilterActionTypeToggleButton),
			                            new PropertyMetadata(default(bool)));

		public static readonly DependencyProperty ColorProperty =
			DependencyProperty.Register("Color", typeof (bool), typeof (QuickFilterActionTypeToggleButton),
			                            new PropertyMetadata(default(bool)));

		static QuickFilterActionTypeToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (QuickFilterActionTypeToggleButton),
			                                         new FrameworkPropertyMetadata(typeof (QuickFilterActionTypeToggleButton)));
		}

		public QuickFilterActionType FilterActionType
		{
			get { return (QuickFilterActionType) GetValue(FilterActionTypeProperty); }
			set { SetValue(FilterActionTypeProperty, value); }
		}

		public bool Include
		{
			get { return (bool) GetValue(IncludeProperty); }
			set { SetValue(IncludeProperty, value); }
		}

		public bool Exclude
		{
			get { return (bool) GetValue(ExcludeProperty); }
			set { SetValue(ExcludeProperty, value); }
		}

		public bool Color
		{
			get { return (bool) GetValue(ColorProperty); }
			set { SetValue(ColorProperty, value); }
		}
	}
}