using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using SharpTail.Ui.ViewModels;

namespace SharpTail.Ui.Controls
{
	public sealed class HighlightableTextBlock
		: TextBlock
	{
		public static readonly DependencyProperty FilterStringProperty =
			DependencyProperty.Register("StringFilter", typeof(string), typeof(HighlightableTextBlock),
										new PropertyMetadata(default(string), OnFilterStringChanged));

		public static readonly DependencyProperty LogEntryProperty =
			DependencyProperty.Register("LogEntry", typeof(LogEntryViewModel), typeof(HighlightableTextBlock),
										new PropertyMetadata(default(LogEntryViewModel), OnLogEntryChanged));

		public LogEntryViewModel LogEntry
		{
			get { return (LogEntryViewModel)GetValue(LogEntryProperty); }
			set { SetValue(LogEntryProperty, value); }
		}

		public string FilterString
		{
			get { return (string)GetValue(FilterStringProperty); }
			set { SetValue(FilterStringProperty, value); }
		}

		private static void OnFilterStringChanged(DependencyObject dependencyObject,
												  DependencyPropertyChangedEventArgs e)
		{
			((HighlightableTextBlock)dependencyObject).OnFilterStringChanged((string)e.NewValue);
		}

		private void OnFilterStringChanged(string filterString)
		{
			UpdateInlineBlocks(LogEntry, filterString);
		}

		private static void OnLogEntryChanged(DependencyObject dependencyObject,
											  DependencyPropertyChangedEventArgs e)
		{
			((HighlightableTextBlock)dependencyObject).OnLogEntryChanged((LogEntryViewModel)e.NewValue);
		}

		private void OnLogEntryChanged(LogEntryViewModel logEntry)
		{
			UpdateInlineBlocks(logEntry, FilterString);
		}

		private void UpdateInlineBlocks(LogEntryViewModel logEntry, string filterString)
		{
			Inlines.Clear();
			if (logEntry == null)
			{
				// Nothing to do...
			}
			else
			{
				var message = logEntry.Message;
				if (string.IsNullOrEmpty(filterString))
				{
					var item = new Run(message);
					Inlines.Add(item);
				}
				else
				{
					int idx = 0;
					int last;
					while (true)
					{
						last = idx;
						idx = message.IndexOf(filterString, idx, StringComparison.InvariantCultureIgnoreCase);
						if (idx == -1)
						{
							break;
						}

						TryAddFromTo(message, last, idx);
						var run = AddFromLength(message, idx, filterString.Length);
						run.Background = Brushes.LightBlue;

						idx += filterString.Length;
					}

					TryAddFromTo(message, last, message.Length);
				}
			}
		}

		private Inline AddFromLength(string message, int from, int length)
		{
			var substr = message.Substring(from, length);
			var item = new Run(substr);
			Inlines.Add(item);
			return item;
		}

		private Inline TryAddFromTo(string message, int from, int to)
		{
			int diff = to - from;
			if (diff >= 1)
			{
				return AddFromLength(message, from, diff);
			}

			return null;
		}
	}
}