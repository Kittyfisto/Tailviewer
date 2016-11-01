using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Metrolib;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     This class
	/// </summary>
	public sealed class LogDataCache
	{
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
			}
		}

		/// <summary>
		/// Tests if there is a line from the source and index has been cached.
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
			lock (_syncRoot)
			{
				_logEntries.Add(logTable, index, logEntry);
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
		/// Tests if there is a line from the source and index has been cached.
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
			private struct Key : IEquatable<Key>
			{
				private readonly int _hashCode;
				private readonly TIndex _index;
				private readonly TSource _source;

				public Key(TSource source, TIndex index)
				{
					_source = source;
					_index = index;
					unchecked
					{
						_hashCode = (_source.GetHashCode()*397) ^
						            _index.GetHashCode();
					}
				}

				public bool Equals(Key other)
				{
					return Equals(_source, other._source) && _index.Equals(other._index);
				}

				public override string ToString()
				{
					return string.Format("{0}: {1}", _source, _index);
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