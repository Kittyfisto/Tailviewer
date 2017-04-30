using System;
using System.Collections.ObjectModel;
using System.Windows;
using Tailviewer.BusinessLogic.LogTables;

namespace TablePlayground
{
	public partial class LogTableView
		: ILogTableListener
	{
		private readonly ObservableCollection<TableHeaderItem> _headerItems;

		public LogTableView()
		{
			_headerItems = new ObservableCollection<TableHeaderItem>();

			InitializeComponent();

			PART_VerticalScrollbar.ValueChanged += OnVerticalScrollChanged;
			PART_Header.ItemsSource = _headerItems;
		}

		private void OnVerticalScrollChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
		{
			
		}

		public static readonly DependencyProperty LogTableProperty =
			DependencyProperty.Register("LogTable", typeof (ILogTable), typeof (LogTableView),
			                            new PropertyMetadata(null, OnLogTableChanged));

		public ILogTable LogTable
		{
			get { return (ILogTable) GetValue(LogTableProperty); }
			set { SetValue(LogTableProperty, value); }
		}

		private static void OnLogTableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((LogTableView) d).OnLogTableChanged((ILogTable)e.OldValue, (ILogTable) e.NewValue);
		}

		private void OnLogTableChanged(ILogTable oldValue, ILogTable newValue)
		{
			if (oldValue != null)
			{
				oldValue.RemoveListener(this);
			}

			if (newValue != null)
			{
				newValue.AddListener(this, TimeSpan.FromMilliseconds(100), 10000);
			}
		}

		public void OnLogTableModified(ILogTable logTable, LogTableModification modification)
		{
			if (modification.Schema != null)
			{
				Dispatcher.BeginInvoke(new Action(() => OnSchemaChanged(modification.Schema)));
			}
			else if (modification.IsInvalidate)
			{
				Dispatcher.BeginInvoke(new Action(() => OnInvalidated(modification.Section)));
			}
			else
			{
				Dispatcher.BeginInvoke(new Action(() => OnAdded(modification.Section)));
			}
		}

		private void OnAdded(LogTableSection section)
		{
			
		}

		private void OnInvalidated(LogTableSection section)
		{
			
		}

		private void OnSchemaChanged(ILogTableSchema schema)
		{
			_headerItems.Clear();
			foreach (var header in schema.ColumnHeaders)
			{
				_headerItems.Add(new TableHeaderItem(header));
			}
		}
	}
}
