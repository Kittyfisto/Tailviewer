using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Metrolib;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.LogTables;
using log4net;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     This class is responsible for holding a set of <see cref="LogLine" />s and <see cref="LogEntry" />s
	///     in memory. It is a volatile cache that may cause entries to be removed from the cache over time, thus
	///     what was once added must not be present in the cache over its lifetime.
	/// </summary>
	/// <remarks>
	///     This implementation aims to not consume more than a given amount of memory, for example 50Mb.
	///     This maximum should be taken with a grain of sault because of the amount of assumptions made about
	///     the .NET framework implementation (which is subject to change). On top of that, object graphs of
	///     <see cref="LogEntry.Fields" /> are not fully followed and object re-use between different
	///     <see cref="LogEntry.Fields" /> is not detected.
	/// </remarks>
	public sealed class LogDataCache
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly LogEntryCache _logEntries;
		private readonly LogLineCache _logLines;
		private readonly Size _maximumSize;
		private readonly object _syncRoot;

		public LogDataCache()
		{
			_maximumSize = Size.FromMegabytes(200);
			_syncRoot = new object();
			_logLines = new LogLineCache();
			_logEntries = new LogEntryCache();
		}

		public Size Size
		{
			get { return _logEntries.Size + _logLines.Size; }
		}

		public int Count
		{
			get { return _logEntries.Count + _logLines.Count; }
		}

		#region LogFile

		/// <summary>
		///     Adds the given line to this cache.
		///     If a line already exists, then it will be replaced.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="index"></param>
		/// <param name="logLine"></param>
		public void Add(ILogFile logFile, LogLineIndex index, LogLine logLine)
		{
			lock (_syncRoot)
			{
				_logLines.Add(logFile, index, logLine);
				CollectIfNecessary();
			}
		}

		/// <summary>
		///     Adds the given range of entries to this cache.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="startIndex"></param>
		/// <param name="logLines"></param>
		public void AddRange(ILogFile logFile, LogLineIndex startIndex, IEnumerable<LogLine> logLines)
		{
			if (logFile == null)
				throw new ArgumentNullException("logFile");

			lock (_syncRoot)
			{
				LogLineIndex index = startIndex;
				foreach (var lineLine in logLines)
				{
					_logLines.Add(logFile, index, lineLine);
					++index;
				}

				// We DO NOT want to collect inside the loop.
				// Not doing so breaks the promise of not consuming more memory
				// than configured, however that is a very weak promise from the start
				// and thus we do not bother at trying to *strictly* keep it. Instead we
				// see it as a guideline not not exceed it too often or by too much (tm).
				CollectIfNecessary();
			}
		}

		/// <summary>
		///     Tests if there is a line from the source and index has been cached.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		[Pure]
		public bool Contains(ILogFile logFile, LogLineIndex index)
		{
			if (logFile == null)
				return false;
			if (index == LogLineIndex.Invalid)
				return false;

			lock (_syncRoot)
			{
				return _logLines.Contains(logFile, index);
			}
		}

		/// <summary>
		///     Removes all log line associated with the given file.
		/// </summary>
		/// <param name="logFile"></param>
		public int Remove(ILogFile logFile)
		{
			if (logFile == null)
				return 0;

			lock (_syncRoot)
			{
				return _logLines.Remove(logFile);
			}
		}

		/// <summary>
		///     Removes the given line from this cache.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="index"></param>
		/// <returns>true if the entry was removed, false otherwise</returns>
		public bool Remove(ILogFile logFile, LogLineIndex index)
		{
			lock (_syncRoot)
			{
				return _logLines.Remove(logFile, index);
			}
		}

		/// <summary>
		///     Tries to retrieve a log line from this cache.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="index"></param>
		/// <param name="logLine"></param>
		/// <returns></returns>
		public bool TryGetValue(ILogFile logFile, LogLineIndex index, out LogLine logLine)
		{
			lock (_syncRoot)
			{
				return _logLines.TryGetValue(logFile, index, out logLine);
			}
		}

		#endregion

		#region LogTable

		/// <summary>
		///     Adds the given entry to this cache.
		///     If an entry already exists, then it will be replaced.
		/// </summary>
		/// <param name="logTable"></param>
		/// <param name="index"></param>
		/// <param name="logEntry"></param>
		public void Add(ILogTable logTable, LogEntryIndex index, LogEntry logEntry)
		{
			if (logTable == null)
				throw new ArgumentNullException("logTable");

			lock (_syncRoot)
			{
				_logEntries.Add(logTable, index, logEntry);
				CollectIfNecessary();
			}
		}

		/// <summary>
		///     Adds the given range of entries to this cache.
		/// </summary>
		/// <param name="logTable"></param>
		/// <param name="startIndex"></param>
		/// <param name="logEntries"></param>
		public void AddRange(ILogTable logTable, LogEntryIndex startIndex, IEnumerable<LogEntry> logEntries)
		{
			if (logTable == null)
				throw new ArgumentNullException("logTable");

			lock (_syncRoot)
			{
				LogEntryIndex index = startIndex;
				foreach (var logEntry in logEntries)
				{
					_logEntries.Add(logTable, index, logEntry);
					++index;
				}

				// We DO NOT want to collect inside the loop.
				// Not doing so breaks the promise of not consuming more memory
				// than configured, however that is a very weak promise from the start
				// and thus we do not bother at trying to *strictly* keep it. Instead we
				// see it as a guideline not not exceed it too often or by too much (tm).
				CollectIfNecessary();
			}
		}

		/// <summary>
		///     Removes all entries associated with the given table.
		/// </summary>
		/// <param name="logTable"></param>
		/// <returns></returns>
		public int Remove(ILogTable logTable)
		{
			if (logTable == null)
				return 0;

			lock (_syncRoot)
			{
				return _logEntries.Remove(logTable);
			}
		}

		/// <summary>
		///     Removes the given entry from this cache.
		/// </summary>
		/// <param name="logTable"></param>
		/// <param name="index"></param>
		/// <returns>true if the entry was removed, false otherwise</returns>
		public bool Remove(ILogTable logTable, LogEntryIndex index)
		{
			lock (_syncRoot)
			{
				return _logEntries.Remove(logTable, index);
			}
		}

		/// <summary>
		///     Tests if there is a line from the source and index has been cached.
		/// </summary>
		/// <param name="logTable"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		[Pure]
		public bool Contains(ILogTable logTable, LogEntryIndex index)
		{
			if (logTable == null)
				return false;
			if (index == LogEntryIndex.Invalid)
				return false;

			lock (_syncRoot)
			{
				return _logEntries.Contains(logTable, index);
			}
		}

		/// <summary>
		///     Tries to retrieve an entry from this cache.
		/// </summary>
		/// <param name="logTable"></param>
		/// <param name="index"></param>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		public bool TryGetValue(ILogTable logTable, LogEntryIndex index, out LogEntry logEntry)
		{
			lock (_syncRoot)
			{
				return _logEntries.TryGetValue(logTable, index, out logEntry);
			}
		}

		#endregion

		/// <summary>
		///     Checks if the configured maximum amount of memory is exceeded and, if it is,
		///     removes the oldest entries and lines in this cache.
		/// </summary>
		private void CollectIfNecessary()
		{
			Size size = Size;
			Size threshold = size - _maximumSize;
			if (threshold > Size.Zero)
			{
				Size free = threshold + _maximumSize/10;
				Log.DebugFormat("Threshold of {0} has been exceeded by {1}, collecting {2}", _maximumSize,
				                threshold,
				                free);

				var sw = new Stopwatch();
				sw.Start();

				// Most of this code is spent on finding a common representation
				// for the different keys of both caches (one for lines and one for entries).
				// Our goal is to have ONE limit for BOTH caches without prefering either.
				// So when it's time to collect we need to order the list of all keys,
				// keep but the oldest ones and then find out which cache they belong to.
				List<Cache<ILogTable, LogEntryIndex, LogEntry>.LastAccessKey> entries = _logEntries.LastAccessTimes;
				List<Cache<ILogFile, LogLineIndex, LogLine>.LastAccessKey> lines = _logLines.LastAccessTimes;
				List<LastAccessKey> lastAccess = Merge(entries, lines);

				IEnumerable<LastAccessKey> remove = FindOldest(lastAccess, free);
				List<Cache<ILogTable, LogEntryIndex, LogEntry>.Key> outdatedEntries;
				List<Cache<ILogFile, LogLineIndex, LogLine>.Key> outdatedLines;
				Split(remove, out outdatedEntries, out outdatedLines);

				_logLines.Remove(outdatedLines);
				_logEntries.Remove(outdatedEntries);

				Log.DebugFormat("Removed a total of {0} log lines and {1} log entries in {2}ms",
				                outdatedLines.Count,
				                outdatedEntries.Count,
				                sw.Elapsed);
			}
		}

		/// <summary>
		///     Finds the oldest entries in the given list that amount to the given amount of size.
		/// </summary>
		/// <param name="lastAccess"></param>
		/// <param name="minimiumSize"></param>
		/// <returns></returns>
		private IEnumerable<LastAccessKey> FindOldest(List<LastAccessKey> lastAccess, Size minimiumSize)
		{
			lastAccess.Sort(new AscendingDateTimeComparer());
			Size size = Size.Zero;
			int i;
			for (i = 0; i < lastAccess.Count; ++i)
			{
				size += lastAccess[i].Size;
				if (size >= minimiumSize)
					break;
			}

			var ret = new LastAccessKey[i];
			lastAccess.CopyTo(0, ret, 0, i);
			return ret;
		}

		private List<LastAccessKey> Merge(List<Cache<ILogTable, LogEntryIndex, LogEntry>.LastAccessKey> entries,
		                                  List<Cache<ILogFile, LogLineIndex, LogLine>.LastAccessKey> lines)
		{
			var ret = new List<LastAccessKey>(entries.Count + lines.Count);
			foreach (Cache<ILogTable, LogEntryIndex, LogEntry>.LastAccessKey entry in entries)
			{
				ret.Add(new LastAccessKey(entry));
			}
			foreach (Cache<ILogFile, LogLineIndex, LogLine>.LastAccessKey line in lines)
			{
				ret.Add(new LastAccessKey(line));
			}
			return ret;
		}

		private void Split(IEnumerable<LastAccessKey> merged, out List<Cache<ILogTable, LogEntryIndex, LogEntry>.Key> entries,
		                   out List<Cache<ILogFile, LogLineIndex, LogLine>.Key> lines)
		{
			entries = new List<Cache<ILogTable, LogEntryIndex, LogEntry>.Key>();
			lines = new List<Cache<ILogFile, LogLineIndex, LogLine>.Key>();

			foreach (LastAccessKey key in merged)
			{
				if (key.EntryKey.Source != null)
				{
					entries.Add(key.EntryKey);
				}
				else
				{
					lines.Add(key.LineKey);
				}
			}
		}

		private sealed class AscendingDateTimeComparer
			: IComparer<LastAccessKey>
		{
			public int Compare(LastAccessKey x, LastAccessKey y)
			{
				return x.LastAccess.CompareTo(y.LastAccess);
			}
		}

		private abstract class Cache<TSource, TIndex, TValue>
		{
			private readonly Dictionary<Key, CacheEntry> _values;
			private Size _size;

			protected Cache()
			{
				_values = new Dictionary<Key, CacheEntry>();
			}

			public Size Size
			{
				get { return _size; }
			}

			public int Count
			{
				get { return _values.Count; }
			}

			public List<LastAccessKey> LastAccessTimes
			{
				get
				{
					var ret = new List<LastAccessKey>(_values.Count);
					foreach (var pair in _values)
					{
						ret.Add(new LastAccessKey(pair.Key, pair.Value.LastAccess, pair.Value.Size));
					}
					return ret;
				}
			}

			protected abstract Size EstimateSize(TValue value);

			public void Add(TSource logTable, TIndex index, TValue value)
			{
				var key = new Key(logTable, index);
				CacheEntry entry;
				if (!_values.TryGetValue(key, out entry))
				{
					entry = new CacheEntry(value, EstimateSize(value));
					_values.Add(key, entry);
				}
				else
				{
					_size -= entry.Size;
					_values[key] = entry;
				}

				_size += entry.Size;
			}

			[Pure]
			public bool Contains(TSource source, TIndex index)
			{
				var key = new Key(source, index);
				return _values.ContainsKey(key);
			}

			public int Remove(TSource source)
			{
				int count = 0;
				foreach (var pair in _values.ToList())
				{
					if (Equals(pair.Key.Source, source))
					{
						_values.Remove(pair.Key);
						_size -= pair.Value.Size;
						++count;
					}
				}
				return count;
			}

			public void Remove(IEnumerable<Key> keys)
			{
				foreach (Key key in keys)
				{
					_values.Remove(key);
				}
			}

			public bool Remove(TSource source, TIndex index)
			{
				var key = new Key(source, index);
				return _values.Remove(key);
			}

			public bool TryGetValue(TSource source, TIndex index, out TValue value)
			{
				var key = new Key(source, index);
				CacheEntry entry;
				if (_values.TryGetValue(key, out entry))
				{
					value = entry.Value;
					return true;
				}

				value = default(TValue);
				return false;
			}

			/// <summary>
			///     A single entry in the cache.
			/// </summary>
			private sealed class CacheEntry
			{
				public readonly Size Size;
				private readonly TValue _value;
				private DateTime _lastAccess;

				public CacheEntry(TValue value, Size size)
				{
					_lastAccess = DateTime.Now;
					Size = size;
					_value = value;
				}

				public DateTime LastAccess
				{
					get { return _lastAccess; }
				}

				public TValue Value
				{
					get
					{
						_lastAccess = DateTime.Now;
						return _value;
					}
				}

				public override string ToString()
				{
					return string.Format("{0} ({1}", _value, Size);
				}
			}

			/// <summary>
			///     Represents a single line in the log data cache.
			/// </summary>
			public struct Key : IEquatable<Key>
			{
				public readonly TSource Source;
				private readonly int _hashCode;
				private readonly TIndex _index;

				public Key(TSource source, TIndex index)
				{
					Source = source;
					_index = index;
					unchecked
					{
						_hashCode = (Source.GetHashCode()*397) ^
						            _index.GetHashCode();
					}
				}

				public bool Equals(Key other)
				{
					return Equals(Source, other.Source) && _index.Equals(other._index);
				}

				public override string ToString()
				{
					return string.Format("{0}: {1}", Source, _index);
				}

				public override bool Equals(object obj)
				{
					if (ReferenceEquals(null, obj)) return false;
					return obj is Key && Equals((Key) obj);
				}

				public override int GetHashCode()
				{
					return _hashCode;
				}

				public static bool operator ==(Key left, Key right)
				{
					return left.Equals(right);
				}

				public static bool operator !=(Key left, Key right)
				{
					return !left.Equals(right);
				}
			}

			public struct LastAccessKey
			{
				public readonly Key Key;
				public readonly DateTime LastAccess;
				public readonly Size Size;

				public LastAccessKey(Key key, DateTime lastAccess, Size size)
				{
					Key = key;
					LastAccess = lastAccess;
					Size = size;
				}
			}
		}

		private struct LastAccessKey
		{
			public readonly Cache<ILogTable, LogEntryIndex, LogEntry>.Key EntryKey;
			public readonly DateTime LastAccess;
			public readonly Cache<ILogFile, LogLineIndex, LogLine>.Key LineKey;
			public readonly Size Size;

			public LastAccessKey(Cache<ILogTable, LogEntryIndex, LogEntry>.LastAccessKey entry)
			{
				EntryKey = entry.Key;
				LineKey = default(Cache<ILogFile, LogLineIndex, LogLine>.Key);
				LastAccess = entry.LastAccess;
				Size = entry.Size;
			}

			public LastAccessKey(Cache<ILogFile, LogLineIndex, LogLine>.LastAccessKey line)
			{
				EntryKey = default(Cache<ILogTable, LogEntryIndex, LogEntry>.Key);
				LineKey = line.Key;
				LastAccess = line.LastAccess;
				Size = line.Size;
			}
		}

		private sealed class LogEntryCache
			: Cache<ILogTable, LogEntryIndex, LogEntry>
		{
			protected override Size EstimateSize(LogEntry value)
			{
				// TODO: Add proper size estimation
				return Size.OneByte;
			}
		}

		private sealed class LogLineCache
			: Cache<ILogFile, LogLineIndex, LogLine>
		{
			protected override Size EstimateSize(LogLine value)
			{
				// TODO: Add proper size estimation
				return Size.OneByte;
			}
		}
	}
}