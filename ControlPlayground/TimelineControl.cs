using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ControlPlayground
{
	[TemplatePart(Name = "PART_Thumb1", Type = typeof(Thumb))]
	[TemplatePart(Name = "PART_Thumb2", Type = typeof(Thumb))]
	public sealed class TimelineControl
		: Control
	{
		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
			"Minimum", typeof(DateTime), typeof(TimelineControl), new PropertyMetadata(default(DateTime)));

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
			"Maximum", typeof(DateTime), typeof(TimelineControl), new PropertyMetadata(default(DateTime)));

		public static readonly DependencyProperty CurrentMinimumValueProperty = DependencyProperty.Register(
			"CurrentMinimumValue", typeof(DateTime), typeof(TimelineControl), new PropertyMetadata(default(DateTime)));

		public static readonly DependencyProperty CurrentMaximumValueProperty = DependencyProperty.Register(
			"CurrentMaximumValue", typeof(DateTime), typeof(TimelineControl), new PropertyMetadata(default(DateTime)));

		private Thumb _thumb1;
		private Thumb _thumb2;

		static TimelineControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineControl),
				new FrameworkPropertyMetadata(typeof(TimelineControl)));
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_thumb1 = (Thumb)GetTemplateChild("PART_Thumb1");
			_thumb1.DragStarted += Thumb1OnDragStarted;

			_thumb2 = (Thumb)GetTemplateChild("PART_Thumb2");
			_thumb2.DragStarted += Thumb2OnDragStarted;
		}

		private void Thumb2OnDragStarted(object sender, DragStartedEventArgs dragStartedEventArgs)
		{
			int n = 0;
		}

		private void Thumb1OnDragStarted(object sender, DragStartedEventArgs dragStartedEventArgs)
		{
			int n = 0;
		}

		public DateTime CurrentMinimumValue
		{
			get { return (DateTime) GetValue(CurrentMinimumValueProperty); }
			set { SetValue(CurrentMinimumValueProperty, value); }
		}

		public DateTime CurrentMaximumValue
		{
			get { return (DateTime) GetValue(CurrentMaximumValueProperty); }
			set { SetValue(CurrentMaximumValueProperty, value); }
		}

		public DateTime Minimum
		{
			get { return (DateTime) GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}

		public DateTime Maximum
		{
			get { return (DateTime) GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}
	}
}