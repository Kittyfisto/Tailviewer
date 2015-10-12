using System.Windows;
using System.Windows.Controls;

namespace SharpTail.Ui.Controls
{
	public class LogViewerControl : Control
	{
		private ListView _partListView;

		public static readonly DependencyProperty FollowTailProperty =
			DependencyProperty.Register("FollowTail", typeof (bool),
			typeof (LogViewerControl),
			new PropertyMetadata(false, OnFollowTailChanged));

		private static void OnFollowTailChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			((LogViewerControl) dependencyObject).OnFollowTailChanged((bool) e.NewValue);
		}

		private void OnFollowTailChanged(bool followTail)
		{
			var view = _partListView;
			if (view != null)
			{
				if (followTail)
				{
					var items = view.Items;
					view.SelectedItem = items.GetItemAt(items.Count - 1);
					view.ScrollIntoView(view.SelectedItem);
				}
			}
		}

		public bool FollowTail
		{
			get { return (bool) GetValue(FollowTailProperty); }
			set { SetValue(FollowTailProperty, value); }
		}

		static LogViewerControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (LogViewerControl),
			                                         new FrameworkPropertyMetadata(typeof (LogViewerControl)));
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_partListView = (ListView)GetTemplateChild("PART_ListView");
		}
	}
}