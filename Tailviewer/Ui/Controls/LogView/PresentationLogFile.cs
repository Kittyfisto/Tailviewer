using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Ui.Controls.LogView
{
	/// <summary>
	///     An <see cref="ILogFile" /> implementation which provides additional columns for the log file's
	///     eventual presentation.
	/// </summary>
	/// <remarks>
	///     Keeps track of the number of lines a log entry requires for its presentation, the total number of line
	///     in its document and the maximum width to display those log entries.
	/// </remarks>
	public sealed class PresentationLogFile
		: AbstractLogFile
		, ILogFileListener
	{
		const int MaximumLineCount = 1000;

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly LogEntryBuffer _buffer;

		private readonly ConcurrentQueue<PendingModification> _pendingModifications;
		private readonly LogEntryList _indices;
		private readonly TimeSpan _maximumWaitTime;
		private readonly object _syncRoot;
		private readonly ILogFile _source;

		private float _maxWidth;
		private int _lineCount;

		public PresentationLogFile(ITaskScheduler scheduler, ILogFile source)
			: this(scheduler, source, TimeSpan.FromMilliseconds(value: 100))
		{
		}

		public PresentationLogFile(ITaskScheduler scheduler, ILogFile source, TimeSpan maximumWaitTime)
			: base(scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			_maximumWaitTime = maximumWaitTime;
			_source = source;

			_indices = new LogEntryList(IndexedColumns);

			_buffer = new LogEntryBuffer(MaximumLineCount, LogFileColumns.RawContent);
			_pendingModifications = new ConcurrentQueue<PendingModification>();
			_syncRoot = new object();
			
			_source.AddListener(this, _maximumWaitTime, MaximumLineCount);
			StartTask();
		}

		[Pure]
		private static bool IsIndexedColumn(ILogFileColumn column)
		{
			return IndexedColumns.Contains(column);
		}

		/// <summary>
		///     The columns which are actually stored in this log file.
		/// </summary>
		private static IReadOnlyList<ILogFileColumn> IndexedColumns => new ILogFileColumn[]
		{
			LogFileColumns.PresentationStartingLineNumber,
			LogFileColumns.PresentationLineCount,
			LogFileColumns.RawContentMaxPresentationWidth
		};

		public double MaximumWidth => _maxWidth;

		public int LineCount => _lineCount;

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			_pendingModifications.Enqueue(new PendingModification(logFile, section));
		}

		private void Update(ILogFile logFile, LogFileSection section)
		{
			try
			{
				if (section.IsReset)
					Clear(logFile);
				else if (section.IsInvalidate)
					InvalidateFrom(logFile, section.Index);
				else
					Add(logFile, section);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		private void Clear(ILogFile logFile)
		{
			lock (_syncRoot)
			{
				if (!ReferenceEquals(logFile, _source))
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

		private void InvalidateFrom(ILogFile logFile, LogLineIndex index)
		{
			lock (_syncRoot)
			{
				if (!ReferenceEquals(logFile, _source))
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

		private void Add(ILogFile logFile, LogFileSection section)
		{
			// !!!We deliberately retrieve this section OUTSIDE of our own lock!!!
			logFile.GetEntries(section, _buffer);

			// Calculating the max width of a line takes time and is therefore done outside
			// the lock!
			var indices = new List<ILogEntry>(section.Count);
			for (var i = 0; i < section.Count; ++i)
			{
				var logEntry = _buffer[i];
				indices.Add(CreateIndex(logEntry));
			}

			lock (_syncRoot)
			{
				if (!ReferenceEquals(logFile, _source))
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
							var maxWidth = last.GetValue(LogFileColumns.PresentationStartingLineNumber) +
							               last.GetValue(LogFileColumns.PresentationLineCount);
							index.SetValue(LogFileColumns.PresentationStartingLineNumber, maxWidth);
						}
						_indices.Add(index);
						_maxWidth = Math.Max(_maxWidth, index.GetValue(LogFileColumns.RawContentMaxPresentationWidth));
						_lineCount += index.GetValue(LogFileColumns.PresentationLineCount);
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
				_maxWidth = Math.Max(_maxWidth, tmp.GetValue(LogFileColumns.RawContentMaxPresentationWidth));
				_lineCount += tmp.GetValue(LogFileColumns.PresentationLineCount);
			}
		}

		private ILogEntry CreateIndex(IReadOnlyLogEntry logEntry)
		{
			int numLines;
			var width = EstimateWidth(logEntry.RawContent, out numLines);
			var index = new LogEntry2();
			index.SetValue(LogFileColumns.RawContentMaxPresentationWidth, width);
			index.SetValue(LogFileColumns.PresentationLineCount, numLines);
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
			return (float) TextHelper.EstimateWidthUpperLimit(rawContent);
		}

		private struct PendingModification
		{
			public readonly ILogFile LogFile;
			public readonly LogFileSection Section;

			public PendingModification(ILogFile logFile, LogFileSection section)
			{
				LogFile = logFile;
				Section = section;
			}

			public override string ToString()
			{
				return string.Format("{0}: {1}", LogFile, Section);
			}
		}

		public override int MaxCharactersPerLine
		{
			get { throw new NotImplementedException(); }
		}

		public override int Count => _source.Count;

		public override void GetValues(ILogFileProperties properties)
		{
			throw new NotImplementedException();
		}

		public override void GetColumn<T>(LogFileSection section, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			if (IsIndexedColumn(column))
			{
				_indices.CopyTo(column, (int) section.Index, buffer, destinationIndex, section.Count);
			}
			else
			{
				_source.GetColumn(section, column, buffer, destinationIndex);
			}
		}

		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			if (IsIndexedColumn(column))
			{
				_indices.CopyTo(column, new Int32View(indices), buffer, destinationIndex);
			}
			else
			{
				_source.GetColumn(indices, column, buffer, destinationIndex);
			}
		}

		public override void GetEntries(LogFileSection section, ILogEntries buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public override void GetEntries(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public override void GetSection(LogFileSection section, LogLine[] dest)
		{
			throw new NotImplementedException();
		}

		public override LogLine GetLine(int index)
		{
			throw new NotImplementedException();
		}

		public override double Progress => _source.Progress * MyProgress;

		/// <summary>
		///     TODO: Add better estimation.
		/// </summary>
		private double MyProgress => 1;

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			bool performedWork = false;

			PendingModification pendingModification;
			while (_pendingModifications.TryDequeue(out pendingModification))
			{
				if (pendingModification.LogFile != _source)
				{
					Log.WarnFormat("Ignoring log file modification '{0}, {1}': It's probably from a previous log file",
					               pendingModification.LogFile, pendingModification.Section);
				}
				else
				{
					Update(pendingModification.LogFile, pendingModification.Section);
				}

				performedWork = true;
			}

			return performedWork
				? TimeSpan.Zero
				: _maximumWaitTime;
		}

		public override ErrorFlags Error => _source.Error;

		public override DateTime LastModified => _source.LastModified;

		public override DateTime Created => _source.Created;

		public override IReadOnlyList<ILogFileColumn> Columns => LogFileColumns.Combine(_source.Columns, IndexedColumns);

		public override IReadOnlyList<ILogFilePropertyDescriptor> Properties => _source.Properties;

		public override object GetValue(ILogFilePropertyDescriptor propertyDescriptor)
		{
			return _source.GetValue(propertyDescriptor);
		}

		public override T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			return _source.GetValue(propertyDescriptor);
		}
	}
}