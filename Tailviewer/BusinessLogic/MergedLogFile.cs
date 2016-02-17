using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     Responsible for merging two or more <see cref="ILogFile" /> instances into one coherent view.
	///     <see cref="LogLine" />s which have no <see cref="LogLine.Timestamp" /> set (or who's parent doesn't)
	///     are discarded from this representation.
	/// </summary>
	internal sealed class MergedLogFile
		: ILogFile
		  , ILogFileListener
	{
		private const int BatchSize = 1000;

		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly ManualResetEvent _endOfSectionHandle;

		private readonly List<Index> _indices;

		private readonly LogFileListenerCollection _listeners;
		private readonly ConcurrentQueue<PendingModification> _pendingModifications;
		private readonly Task _readTask;
		private readonly ILogFile[] _sources;
		private readonly object _syncRoot;
		private int _debugCount;
		private int _errorCount;
		private int _fatalCount;
		private Size _fileSize;
		private int _infoCount;
		private DateTime _lastModified;
		private int _otherCount;
		private int _skippedCount;
		private DateTime? _startTimestamp;
		private int _warningCount;

		public MergedLogFile(IEnumerable<ILogFile> sources)
			: this(sources.ToArray())
		{
		}

		public MergedLogFile(params ILogFile[] sources)
		{
			if (sources == null) throw new ArgumentNullException("sources");
			if (sources.Any(x => x == null)) throw new ArgumentException("sources", "sources.Any(x => x == null)");

			_sources = sources;
			_cancellationTokenSource = new CancellationTokenSource();
			_readTask = new Task(Merge,
			                     _cancellationTokenSource.Token,
			                     _cancellationTokenSource.Token,
			                     TaskCreationOptions.LongRunning);
			_listeners = new LogFileListenerCollection(this);
			_pendingModifications = new ConcurrentQueue<PendingModification>();
			_indices = new List<Index>();
			_syncRoot = new object();
			_endOfSectionHandle = new ManualResetEvent(false);
		}

		public IEnumerable<ILogFile> Sources
		{
			get { return _sources; }
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			try
			{
				_readTask.Wait();
			}
			catch (AggregateException e)
			{
				Log.DebugFormat("Caught exception while disposing: {0}", e);
			}
			_readTask.Dispose();
		}

		public int FatalCount
		{
			get { return _fatalCount; }
		}

		public void Wait()
		{
			while (true)
			{
				if (_endOfSectionHandle.WaitOne(TimeSpan.FromMilliseconds(100)))
					break;

				if (_readTask.IsFaulted)
					throw _readTask.Exception;
			}
		}

		public DateTime? StartTimestamp
		{
			get { return _startTimestamp; }
		}

		public DateTime LastModified
		{
			get { return _lastModified; }
		}

		public Size FileSize
		{
			get { return _fileSize; }
		}

		public int Count
		{
			get
			{
				lock (_indices)
				{
					return _indices.Count;
				}
			}
		}

		public int OtherCount
		{
			get { return _otherCount; }
		}

		public int DebugCount
		{
			get { return _debugCount; }
		}

		public int InfoCount
		{
			get { return _infoCount; }
		}

		public int WarningCount
		{
			get { return _warningCount; }
		}

		public int ErrorCount
		{
			get { return _errorCount; }
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void Remove(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			for (int i = 0; i < section.Count; ++i)
			{
				// TODO: This seems rubbish - maybe I should change the interface to SourceLineIndex altogether?
				dest[i] = GetLine((int) (section.Index + i));
			}
		}

		public LogLine GetLine(int index)
		{
			Index idx;
			lock (_indices)
			{
				idx = _indices[index];
			}

			LogLine line = idx.LogFile.GetLine(idx.SourceLineIndex);
			var actualLine = new LogLine(index,
			                             idx.MergedLogEntryIndex,
			                             line);
			return actualLine;
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			_endOfSectionHandle.Reset();
			_pendingModifications.Enqueue(new PendingModification(logFile, section));
		}

		private void Merge(object obj)
		{
			CancellationToken token = _cancellationTokenSource.Token;

			while (!token.IsCancellationRequested)
			{
				PendingModification modification;
				if (_pendingModifications.TryDequeue(out modification))
				{
					if (modification.Section.IsReset)
					{
						Clear();
					}
					else if (modification.Section.InvalidateSection)
					{
						// This one only needs to be implemented when MergedLogFiles use other
						// MergedLogFiles as source.
						throw new NotImplementedException();
					}
					else
					{
						for (int i = 0; i < modification.Section.Count; ++i)
						{
							LogLineIndex sourceIndex = modification.Section.Index + i;
							LogLine newLogLine = modification.LogFile.GetLine((int) sourceIndex);
							if (newLogLine.Timestamp != null)
							{
								// We need to find out where this new entry (or entries) is/are to be inserted.
								int insertionIndex = _indices.Count;
								for (int n = _indices.Count - 1; n >= 0; --n)
								{
									Index idx = _indices[n];
									LogLine entry = idx.LogFile.GetLine(idx.SourceLineIndex);
									if (entry.Timestamp <= newLogLine.Timestamp)
									{
										insertionIndex = n + 1;
										break;
									}
									if (entry.Timestamp > newLogLine.Timestamp)
									{
										// We know that we MIGHT have to insert the new item *before*
										// the current entry, but we can't stop looking yet until
										// we've either reached the first entry or find an entry
										// that is *before* the new entry... => hence no break here!
										insertionIndex = n;
									}
								}

								int mergedLogEntryIndex = GetMergedLogEntryIndex(modification.LogFile, insertionIndex, newLogLine);
								var index = new Index((int) sourceIndex,
								                      mergedLogEntryIndex,
								                      newLogLine.LogEntryIndex,
								                      modification.LogFile);
								if (insertionIndex < _indices.Count)
								{
									InvalidateOnward(insertionIndex, modification.LogFile, newLogLine);
								}

								lock (_syncRoot)
								{
									_indices.Insert(insertionIndex, index);
								}

								UpdateCounts(newLogLine);

								_listeners.OnRead(_indices.Count);
							}
							else
							{
								// LogEntries which do not have a timestamp associated with them cannot be 
								// used in a merged view and thus have to be skipped.
								++_skippedCount;
							}
						}
					}
				}
				else
				{
					_fileSize = _sources.Aggregate(Size.Zero, (a, file) => a + file.FileSize);
					_lastModified = _sources.Aggregate(DateTime.MinValue,
					                                   (a, file) =>
						                                   {
							                                   DateTime modified = file.LastModified;
							                                   if (modified > a)
								                                   return modified;

							                                   return a;
						                                   }
						);
					_startTimestamp = _sources.Aggregate((DateTime?) null,
					                                     (a, file) =>
						                                     {
							                                     DateTime? startTime = file.StartTimestamp;
							                                     if (startTime == null)
								                                     return a;
							                                     if (a == null)
								                                     return startTime;
							                                     if (startTime < a)
								                                     return startTime;
							                                     return a;
						                                     }
						);

					_listeners.OnRead(_indices.Count);
					_endOfSectionHandle.Set();
					Thread.Sleep(TimeSpan.FromMilliseconds(10));
				}
			}
		}

		/// <summary>
		///     Finds the log entry index for the given log line in this merged data structure.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="insertionIndex"></param>
		/// <param name="newLogLine"></param>
		/// <returns></returns>
		[Pure]
		private int GetMergedLogEntryIndex(ILogFile logFile, int insertionIndex, LogLine newLogLine)
		{
			if (insertionIndex > 0)
			{
				Index previousLine = _indices[insertionIndex - 1];
				if (previousLine.LogFile == logFile &&
				    previousLine.OriginalLogEntryIndex == newLogLine.LogEntryIndex)
				{
					return previousLine.MergedLogEntryIndex;
				}

				return previousLine.MergedLogEntryIndex + 1;
			}

			return 0;
		}

		private void InvalidateOnward(int insertionIndex, ILogFile source, LogLine newLogLine)
		{
			// If the new entry is to be inserted anywhere else, then we need to invalidate
			// everything from that index on, insert the new line at the given index and then
			// issue another modification that includes everything from the newly inserted index
			// to the end.
			int count = _indices.Count - insertionIndex;
			_listeners.Invalidate(insertionIndex, count);

			// This is really interesting.
			// We're inserting a line somewhere in the middle which means that the logentry index of all following
			// entries MAY increase by 1, depending on whether or not the inserted log line is a new entry
			// or belongs to the previous line's entry
			bool patchFollowingIndices = true;
			if (insertionIndex > 0)
			{
				Index previousLine = _indices[insertionIndex - 1];
				if (previousLine.LogFile == source &&
				    previousLine.OriginalLogEntryIndex == newLogLine.LogEntryIndex)
				{
					// We're inserting 
					patchFollowingIndices = false;
				}
			}

			if (patchFollowingIndices)
			{
				for (int i = 0; i < count; ++i)
				{
					Index idx = _indices[insertionIndex + i];
					idx.MergedLogEntryIndex++;
					_indices[insertionIndex + i] = idx;
				}
			}
		}

		private void UpdateCounts(LogLine newEntry)
		{
			switch (newEntry.Level)
			{
				case LevelFlags.Debug:
					++_debugCount;
					break;

				case LevelFlags.Info:
					++_infoCount;
					break;

				case LevelFlags.Warning:
					++_warningCount;
					break;

				case LevelFlags.Error:
					++_errorCount;
					break;

				case LevelFlags.Fatal:
					++_fatalCount;
					break;
			}
		}

		private void Clear()
		{
			lock (_indices)
			{
				_indices.Clear();
			}
			_listeners.OnRead(-1);
		}

		public void Start(TimeSpan maximumWaitTime)
		{
			if (_readTask.Status == TaskStatus.Created)
			{
				foreach (ILogFile logFile in _sources)
				{
					logFile.AddListener(this, maximumWaitTime, BatchSize);
				}
				_readTask.Start();
			}
		}

		/// <summary>
		///     Represents an index in the merged data-structure.
		///     Points towards a particular <see cref="LogLine" /> of a particular
		///     <see cref="ILogFile" />.
		/// </summary>
		private struct Index
		{
			public readonly ILogFile LogFile;
			public readonly int OriginalLogEntryIndex;
			public readonly int SourceLineIndex;
			public int MergedLogEntryIndex;

			public Index(int sourceLineIndex,
			             int mergedLogEntryIndex,
			             int originalLogEntryIndex,
			             ILogFile logFile)
			{
				SourceLineIndex = sourceLineIndex;
				MergedLogEntryIndex = mergedLogEntryIndex;
				OriginalLogEntryIndex = originalLogEntryIndex;
				LogFile = logFile;
			}

			public override string ToString()
			{
				return string.Format("SourceLineIndex: {0}, OriginalLogEntryIndex: {1}, LogFile: {2}, MergedLogEntryIndex: {3}",
				                     SourceLineIndex, OriginalLogEntryIndex, LogFile, MergedLogEntryIndex);
			}
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
				return string.Format("{0} ({1})", Section, LogFile);
			}
		}
	}
}