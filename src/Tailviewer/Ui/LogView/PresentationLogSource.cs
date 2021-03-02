using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.Api;
using Tailviewer.Core;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Sources;
using Tailviewer.Settings;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	///     An <see cref="ILogSource" /> implementation which provides additional columns for the log file's
	///     eventual presentation.
	/// </summary>
	/// <remarks>
	///     Keeps track of the number of lines a log entry requires for its presentation, the total number of line
	///     in its document and the maximum width to display those log entries.
	/// </remarks>
	public sealed class PresentationLogSource
		: AbstractLogSource
		, ILogSourceListener
	{
		const int MaximumLineCount = 1000;

		/// <summary>
		///     The maximum width (in pixels) of the <see cref="GeneralColumns.RawContent"/> column content.
		/// </summary>
		public static readonly IColumnDescriptor<float> RawContentMaxPresentationWidth;

		/// <summary>
		/// The line number of the first line of a log entry's presentation.
		/// </summary>
		public static readonly IColumnDescriptor<int> PresentationStartingLineNumber;

		/// <summary>
		/// The number of lines in a log entry's presentation.
		/// </summary>
		public static readonly IColumnDescriptor<int> PresentationLineCount;

		static PresentationLogSource()
		{
			RawContentMaxPresentationWidth = new WellKnownColumnDescriptor<float>("raw_content_max_presentation_width");
			PresentationStartingLineNumber = new WellKnownColumnDescriptor<int>("presentation_line_number");
			PresentationLineCount = new WellKnownColumnDescriptor<int>("presentation_line_count");
		}

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly LogBufferArray _array;

		private readonly ConcurrentQueue<PendingModification> _pendingModifications;
		private readonly LogBufferList _indices;
		private readonly TimeSpan _maximumWaitTime;
		private readonly TextSettings _textSettings;
		private readonly object _syncRoot;
		private readonly ILogSource _source;

		private float _maxWidth;
		private int _lineCount;

		public PresentationLogSource(ITaskScheduler scheduler, ILogSource source, TextSettings textSettings)
			: this(scheduler, source, TimeSpan.FromMilliseconds(value: 100), textSettings)
		{
		}

		public PresentationLogSource(ITaskScheduler scheduler, ILogSource source, TimeSpan maximumWaitTime, TextSettings textSettings)
			: base(scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			_maximumWaitTime = maximumWaitTime;
			_textSettings = textSettings;
			_source = source;

			_indices = new LogBufferList(IndexedColumns);

			_array = new LogBufferArray(MaximumLineCount, GeneralColumns.RawContent);
			_pendingModifications = new ConcurrentQueue<PendingModification>();
			_syncRoot = new object();
			
			_source.AddListener(this, _maximumWaitTime, MaximumLineCount);
			StartTask();
		}

		[Pure]
		private static bool IsIndexedColumn(IColumnDescriptor column)
		{
			return IndexedColumns.Contains(column);
		}

		/// <summary>
		///     The columns which are actually stored in this log file.
		/// </summary>
		private static IReadOnlyList<IColumnDescriptor> IndexedColumns => new IColumnDescriptor[]
		{
			PresentationStartingLineNumber,
			PresentationLineCount,
			RawContentMaxPresentationWidth
		};

		public double MaximumWidth => _maxWidth;

		public int LineCount => _lineCount;

		public void OnLogFileModified(ILogSource logSource, LogSourceModification modification)
		{
			_pendingModifications.Enqueue(new PendingModification(logSource, modification));
		}

		private void Update(ILogSource logSource, LogSourceModification modification)
		{
			try
			{
				if (modification.IsReset())
					Clear(logSource);
				else if (modification.IsRemoved(out var removedSection))
					Remove(logSource, removedSection.Index);
				else if (modification.IsAppended(out var appendedSection))
					Add(logSource, appendedSection);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		private void Clear(ILogSource logSource)
		{
			lock (_syncRoot)
			{
				if (!ReferenceEquals(logSource, _source))
				{
					Log.WarnFormat("Ignoring Clear: It's probably from a previous log file");
				}
				else
				{
					Clear();
				}
			}
		}

		private void Clear()
		{
			lock (_syncRoot)
			{
				_indices.Clear();
				_maxWidth = 0;
			}
		}

		private void Remove(ILogSource logSource, LogLineIndex index)
		{
			lock (_syncRoot)
			{
				if (!ReferenceEquals(logSource, _source))
				{
					Log.WarnFormat("Ignoring invalidation from '{0}' onwards: It's probably from a previous log file",
					               index);
				}
				else
				{
					var count = _indices.Count - index;
					if (count > 0)
					{
						_indices.RemoveRange((int) index, count);
						DetermineMaxWidthAndLineNumbers();
					}
					else
					{
						Log.WarnFormat("Ignoring reset: index is completely out of bounds");
					}
				}
			}
		}

		private void Add(ILogSource logSource, LogSourceSection section)
		{
			// !!!We deliberately retrieve this section OUTSIDE of our own lock!!!
			logSource.GetEntries(section, _array);

			// Calculating the max width of a line takes time and is therefore done outside
			// the lock!
			var indices = new List<ILogEntry>(section.Count);
			for (var i = 0; i < section.Count; ++i)
			{
				var logEntry = _array[i];
				indices.Add(CreateIndex(logEntry));
			}

			lock (_syncRoot)
			{
				if (!ReferenceEquals(logSource, _source))
				{
					// We've retrieved data from a different log file than we wanted to...
					Log.WarnFormat("Ignoring add '{0}': It's probably from a previous log file", section);
				}
				else
				{
					foreach (var index in indices)
					{
						if (_indices.Count > 0)
						{
							var last = _indices[_indices.Count - 1];
							var maxWidth = last.GetValue(PresentationStartingLineNumber) +
							               last.GetValue(PresentationLineCount);
							index.SetValue(PresentationStartingLineNumber, maxWidth);
						}
						_indices.Add(index);
						_maxWidth = Math.Max(_maxWidth, index.GetValue(RawContentMaxPresentationWidth));
						_lineCount += index.GetValue(PresentationLineCount);
					}
				}
			}
		}

		private void DetermineMaxWidthAndLineNumbers()
		{
			_maxWidth = 0;
			_lineCount = 0;
			foreach (var tmp in _indices)
			{
				_maxWidth = Math.Max(_maxWidth, tmp.GetValue(RawContentMaxPresentationWidth));
				_lineCount += tmp.GetValue(PresentationLineCount);
			}
		}

		private ILogEntry CreateIndex(IReadOnlyLogEntry logEntry)
		{
			int numLines;
			var width = EstimateWidth(logEntry.RawContent, out numLines);
			var index = new LogEntry();
			index.SetValue(RawContentMaxPresentationWidth, width);
			index.SetValue(PresentationLineCount, numLines);
			return index;
		}

		private float EstimateWidth(string rawContent, out int numLines)
		{
			var lines = rawContent.Split('\n');
			numLines = lines.Length;
			return lines.Max(line => EstimateWidth(line));
		}

		private float EstimateWidth(string rawContent)
		{
			return (float) _textSettings.EstimateWidthUpperLimit(rawContent);
		}

		private struct PendingModification
		{
			public readonly ILogSource LogSource;
			public readonly LogSourceModification Modification;

			public PendingModification(ILogSource logSource, LogSourceModification modification)
			{
				LogSource = logSource;
				Modification = modification;
			}

			public override string ToString()
			{
				return string.Format("{0}: {1}", LogSource, Modification);
			}
		}

		public override void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_source.SetProperty(property, value);
		}

		public override void GetAllProperties(IPropertiesBuffer destination)
		{
			throw new NotImplementedException();
		}

		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			if (IsIndexedColumn(column))
			{
				_indices.CopyTo(column, new Int32View(sourceIndices), destination, destinationIndex);
			}
			else
			{
				_source.GetColumn(sourceIndices, column, destination, destinationIndex, queryOptions);
			}
		}

		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			throw new NotImplementedException();
		}

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			bool performedWork = false;

			PendingModification pendingModification;
			while (_pendingModifications.TryDequeue(out pendingModification))
			{
				if (pendingModification.LogSource != _source)
				{
					Log.WarnFormat("Ignoring log file modification '{0}, {1}': It's probably from a previous log file",
					               pendingModification.LogSource, pendingModification.Modification);
				}
				else
				{
					Update(pendingModification.LogSource, pendingModification.Modification);
				}

				performedWork = true;
			}

			return performedWork
				? TimeSpan.Zero
				: _maximumWaitTime;
		}

		public override IReadOnlyList<IColumnDescriptor> Columns => GeneralColumns.Combine(_source.Columns, IndexedColumns);

		public override IReadOnlyList<IReadOnlyPropertyDescriptor> Properties => _source.Properties;

		public override object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			return _source.GetProperty(property);
		}

		public override T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			return _source.GetProperty(property);
		}

		public override void SetProperty(IPropertyDescriptor property, object value)
		{
			_source.SetProperty(property, value);
		}
	}
}