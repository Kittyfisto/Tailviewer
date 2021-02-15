using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using log4net;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.IO;

namespace Tailviewer.Core.LogFiles.Text
{
	/// <summary>
	///     A n<see cref="ILogFile" /> implementation which allows (somewhat) constant time random-access to the lines of a log file without keeping the entire file in memory.
	/// </summary>
	[DebuggerTypeProxy(typeof(LogFileView))]
	internal sealed class StreamingTextLogFile
		: ILogFile
		, ILogFileListener
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IServiceContainer _serviceContainer;
		private readonly TimeSpan _maximumWaitTime;
		private readonly LogFileListenerCollection _listeners;

		#region Data

		private int _lineCount;
		private readonly LogEntryCache _cache;
		private readonly LogEntryList _index;
		private readonly object _syncRoot;
		private readonly ConcurrentLogFilePropertyCollection _properties;
		private string _fileName;

		#endregion

		/// <summary>
		///    Initializes this text log file.
		/// </summary>
		/// <remarks>
		///    Plugin authors are deliberately prevented from calling this constructor directly because it's signature may change
		///    over time. In order to create an instance of this type, simply call <see cref="IServiceContainer.CreateTextLogFile"/>.
		/// </remarks>
		/// <param name="serviceContainer"></param>
		/// <param name="fileName"></param>
		/// <param name="maximumWaitTime"></param>
		/// <param name="format"></param>
		/// <param name="encoding"></param>
		internal StreamingTextLogFile(IServiceContainer serviceContainer,
		                              string fileName,
		                              TimeSpan maximumWaitTime,
		                              ILogFileFormat format,
		                              Encoding encoding)
		{
			_serviceContainer = serviceContainer;
			_maximumWaitTime = maximumWaitTime;

			_listeners = new LogFileListenerCollection(this);

			_fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
			_cache = new LogEntryCache(LogFiles.Columns.LogLevel, LogFiles.Columns.RawContent);
			_index = new LogEntryList(LogFiles.Columns.Timestamp);
			_properties = new ConcurrentLogFilePropertyCollection(LogFiles.Properties.Minimum);
			_properties.SetValue(LogFiles.Properties.Name, _fileName);
			_syncRoot = new object();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return _fileName;
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			// https://github.com/Kittyfisto/Tailviewer/issues/282
			_listeners.Clear();
		}

		/// <inheritdoc />
		public IReadOnlyList<IColumnDescriptor> Columns => LogFiles.Columns.Minimum;

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		/// <inheritdoc />
		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties => _properties.Properties;

		/// <inheritdoc />
		public object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			object value;
			_properties.TryGetValue(property, out value);
			return value;
		}

		/// <inheritdoc />
		public T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			T value;
			_properties.TryGetValue(property, out value);
			return value;
		}

		public void SetProperty(IPropertyDescriptor property, object value)
		{
			throw new NotImplementedException();
		}

		public void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public void GetAllProperties(ILogFileProperties destination)
		{
			_properties.CopyAllValuesTo(destination);
		}

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> indices, IColumnDescriptor<T> column, T[] buffer, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			if (indices == null)
				throw new ArgumentNullException(nameof(indices));
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + indices.Count > buffer.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");


			if (Equals(column, LogFiles.Columns.Index) ||
			    Equals(column, LogFiles.Columns.OriginalIndex))
			{
				GetIndex(indices, (LogLineIndex[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFiles.Columns.LogEntryIndex))
			{
				GetIndex(indices, (LogEntryIndex[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFiles.Columns.LineNumber) ||
			         Equals(column, LogFiles.Columns.OriginalLineNumber))
			{
				GetLineNumber(indices, (int[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFiles.Columns.OriginalDataSourceName))
			{
				GetDataSourceName(indices, (string[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFiles.Columns.Timestamp))
			{
				GetTimestamp(indices, (DateTime?[]) (object) buffer, destinationIndex);
			}
			else
			{
				lock (_syncRoot)
				{
					if (_cache.TryCopyTo(indices, column, buffer, destinationIndex))
						return;
				}

				ReadEntries(indices);

				lock (_syncRoot)
				{
					if (!_cache.TryCopyTo(indices, column, buffer, destinationIndex))
					{
						Log.WarnFormat("Unable to satisfy read request after buffer was filled");
					}
				}
			}
		}

		/// <inheritdoc />
		public void GetEntries(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			lock (_syncRoot)
			{
				if (_cache.TryCopyTo(indices, buffer, destinationIndex))
					return;
			}

			ReadEntries(indices);

			lock (_syncRoot)
			{
				if (!_cache.TryCopyTo(indices, buffer, destinationIndex))
				{
					Log.WarnFormat("Unable to satisfy read request after buffer was filled");
				}
			}
		}

		/// <inheritdoc />
		public double Progress
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			return originalLineIndex;
		}

		#region Random Access to computed values

		private void GetIndex(IReadOnlyList<LogLineIndex> indices, LogLineIndex[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _lineCount)
					{
						buffer[destinationIndex + i] = index;
					}
					else
					{
						buffer[destinationIndex + i] = LogFiles.Columns.Index.DefaultValue;
					}
				}
			}
		}

		private void GetIndex(IReadOnlyList<LogLineIndex> indices, LogEntryIndex[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _lineCount)
					{
						buffer[destinationIndex + i] = new LogEntryIndex((int)index);
					}
					else
					{
						buffer[destinationIndex + i] = LogFiles.Columns.LogEntryIndex.DefaultValue;
					}
				}
			}
		}

		private void GetDataSourceName(IReadOnlyList<LogLineIndex> indices, string[] buffer, int destinationIndex)
		{
			for (int i = 0; i < indices.Count; ++i)
			{
				buffer[destinationIndex + i] = _fileName;
			}
		}

		private void GetLineNumber(IReadOnlyList<LogLineIndex> indices, int[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _lineCount)
					{
						var lineNumber = (int) (index + 1);
						buffer[destinationIndex + i] = lineNumber;
					}
					else
					{
						buffer[destinationIndex + i] = LogFiles.Columns.LineNumber.DefaultValue;
					}
				}
			}
		}

		#endregion

		private void GetTimestamp(IReadOnlyList<LogLineIndex> indices, DateTime?[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				_index.CopyTo(LogFiles.Columns.Timestamp, indices, buffer, destinationIndex);
			}
		}

		private void ReadEntries(IReadOnlyList<LogLineIndex> indices)
		{
		}

		#region Sequential file scan

		private void Remove(LogFileSection intersection)
		{
			lock (_syncRoot)
			{
				_cache.RemoveRange((int) intersection.Index, intersection.Count);
				_lineCount -= intersection.Count;
			}

			_listeners.Invalidate((int) intersection.Index, intersection.Count);
		}

		private void Reset()
		{
		}

		#endregion

	}
}