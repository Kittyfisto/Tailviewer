using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NewDesign.Dashboard;

namespace NewDesign.SidePanel
{
	public sealed class SidePanelControl
		: Control
	{
		public static readonly DependencyProperty SidePanelsProperty = DependencyProperty.Register(
			"SidePanels", typeof(IEnumerable<ISidePanelViewModel>), typeof(SidePanelControl),
			new PropertyMetadata(null, OnSidePanelsChanged));

		private static void OnSidePanelsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((SidePanelControl) dependencyObject).OnSidePanelsChanged((IEnumerable<ISidePanelViewModel>) args.NewValue);
		}

		private void OnSidePanelsChanged(IEnumerable<ISidePanelViewModel> panels)
		{
			if (panels != null)
			{
				foreach (var panel in panels)
				{
					panel.PropertyChanged += PanelOnPropertyChanged;
				}
			}
		}

		private void PanelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case nameof(ISidePanelViewModel.IsSelected):
					var panel = (ISidePanelViewModel) sender;
					if (panel.IsSelected)
						SelectedPanel = panel;
					else
						SelectedPanel = SidePanels.FirstOrDefault(x => x.IsSelected);
					break;
			}
		}

		public static readonly DependencyProperty SelectedPanelProperty = DependencyProperty.Register(
			"SelectedPanel", typeof(ISidePanelViewModel), typeof(SidePanelControl), new PropertyMetadata(null, OnSelectedPanelChanged));

		private static void OnSelectedPanelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((SidePanelControl) dependencyObject).OnSelectedPanelChanged((ISidePanelViewModel)args.OldValue, (ISidePanelViewModel)args.NewValue);
		}

		private void OnSelectedPanelChanged(ISidePanelViewModel oldSelection, ISidePanelViewModel newSelection)
		{
			if (oldSelection != null)
				oldSelection.IsSelected = false;
			if (newSelection != null)
				newSelection.IsSelected = true;
		}

		public ISidePanelViewModel SelectedPanel
		{
			get { return (ISidePanelViewModel) GetValue(SelectedPanelProperty); }
			set { SetValue(SelectedPanelProperty, value); }
		}

		static SidePanelControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SidePanelControl),
				new FrameworkPropertyMetadata(typeof(SidePanelControl)));
		}

		public IEnumerable<ISidePanelViewModel> SidePanels
		{
			get { return (IEnumerable<ISidePanelViewModel>) GetValue(SidePanelsProperty); }
			set { SetValue(SidePanelsProperty, value); }
		}
	}
}