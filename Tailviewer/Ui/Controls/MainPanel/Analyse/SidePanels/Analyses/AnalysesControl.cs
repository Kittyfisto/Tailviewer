using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using log4net;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels.Analyses
{
	[TemplatePart(Name = PART_Analyses, Type = typeof (TreeView))]
	public sealed class AnalysesControl : Control
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private TreeView _partAnalyses;
		public const string PART_Analyses = "PART_Analyses";

		public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
		                                                "SelectedItem", typeof(IAnalysisViewModel),
		                                                typeof(AnalysesControl),
		                                                new PropertyMetadata(null, OnSelectedItemChanged));

		public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
		                                                "ItemsSource", typeof(IEnumerable), typeof(AnalysesControl), new PropertyMetadata(default(IEnumerable)));

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable) GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((AnalysesControl) d).OnSelectedItemChanged((IAnalysisViewModel) e.NewValue);
		}

		private void OnSelectedItemChanged(IAnalysisViewModel selectedItem)
		{
			TreeViewItem treeViewItem = GetTreeViewItem(selectedItem);
			if (treeViewItem != null)
			{
				SelectItem(treeViewItem);
			}
			else
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					TreeViewItem item = GetTreeViewItem(selectedItem);
					if (item != null)
					{
						SelectItem(item);
					}
				}));
			}
		}

		private void SelectItem(TreeViewItem treeViewItem)
		{
			treeViewItem.IsSelected = true;
			treeViewItem.BringIntoView();
		}

		/// <summary>
		///     Yes, please don't make dealing with a tree complicated or anything...
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		private TreeViewItem GetTreeViewItem(IAnalysisViewModel item)
		{
			var partDataSources = _partAnalyses;
			if (partDataSources == null)
			{
				Log.Warn("Unable to get the selected tree view item: _partDataSources is not set!");
				return null;
			}

			ItemContainerGenerator containerGenerator = partDataSources.ItemContainerGenerator;
			return GetTreeViewItem(containerGenerator, item);
		}

		private TreeViewItem GetTreeViewItem(ItemContainerGenerator containerGenerator, IAnalysisViewModel item)
		{
			var container = (TreeViewItem) containerGenerator.ContainerFromItem(item);
			if (container != null)
				return container;

			foreach (object childItem in containerGenerator.Items)
			{
				var parent = containerGenerator.ContainerFromItem(childItem) as TreeViewItem;
				if (parent == null)
					continue;

				container = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
				if (container != null)
					return container;

				container = GetTreeViewItem(parent.ItemContainerGenerator, item);
				if (container != null)
					return container;
			}
			return null;
		}

		public IAnalysisViewModel SelectedItem
		{
			get { return (IAnalysisViewModel) GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_partAnalyses = (TreeView) GetTemplateChild(PART_Analyses);
			_partAnalyses.AllowDrop = true;
			_partAnalyses.SelectedItemChanged += PartAnalysesOnSelectedItemChanged;
		}

		private void PartAnalysesOnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> args)
		{
			SelectedItem = args.NewValue as IAnalysisViewModel;
		}
	}
}
