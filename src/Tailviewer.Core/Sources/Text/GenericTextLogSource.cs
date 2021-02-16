using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;
using Tailviewer.Plugins;

namespace Tailviewer.Core.Sources.Text
{
	/// <summary>
	///     A simple accessor which provides access to log entries produced by a <see cref="ILogEntryParser" />.
	///     Parsing happens on demand when corresponding properties are requested.
	/// </summary>
	internal sealed class GenericTextLogSource
		: ILogSource
	{
		private readonly Dictionary<ILogSourceListener, ListenerProxy> _listeners;
		private readonly ILogEntryParser _parser;
		private readonly ILogSource _source;

		public GenericTextLogSource(ILogSource source,
		                            ILogEntryParser parser)
		{
			_source = source ?? throw new ArgumentNullException(nameof(source));
			_parser = parser;
			_listeners = new Dictionary<ILogSourceListener, ListenerProxy>();
		}

		public IReadOnlyList<IColumnDescriptor> Columns
		{
			get { return _source.Columns.Concat(_parser.Columns).Distinct().ToList(); }
		}

		public void AddListener(ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			// We need to make sure that whoever registers with us is getting OUR reference through
			// their listener, not the source we're wrapping (or they might discard events since they're
			// coming not from the source they subscribed to).
			var proxy = new ListenerProxy(this, listener);
			lock (_listeners)
			{
				_listeners.Add(listener, proxy);
			}

			_source.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogSourceListener listener)
		{
			ListenerProxy proxy;
			lock (_listeners)
			{
				if (!_listeners.TryGetValue(listener, out proxy))
					return;
			}

			_source.RemoveListener(proxy);
		}

		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get { return _source.Properties; }
		}

		public object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			return _source.GetProperty(property);
		}

		public T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			return _source.GetProperty(property);
		}

		public void SetProperty(IPropertyDescriptor property, object value)
		{
			_source.SetProperty(property, value);
		}

		public void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_source.SetProperty(property, value);
		}

		public void GetAllProperties(IPropertiesBuffer destination)
		{
			_source.GetAllProperties(destination);
		}

		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices,
		                         IColumnDescriptor<T> column,
		                         T[] destination,
		                         int destinationIndex,
		                         LogFileQueryOptions queryOptions)
		{
			GetEntries(sourceIndices,
			           new SingleColumnLogBufferView<T>(column, destination, destinationIndex, sourceIndices.Count),
			           destinationIndex: 0, queryOptions);
		}

		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                       ILogBuffer destination,
		                       int destinationIndex,
		                       LogFileQueryOptions queryOptions)
		{
			if (destinationIndex != 0)
				throw new NotImplementedException();

			var rawBuffer = destination.Columns.Contains(LogColumns.RawContent)
				? new LogBufferView(destination, LogColumns.RawContent)
				: (ILogBuffer) new LogBufferArray(sourceIndices.Count);
			_source.GetEntries(sourceIndices, rawBuffer, destinationIndex: 0, queryOptions);

			for (var i = 0; i < rawBuffer.Count; ++i)
			{
				var logEntry = rawBuffer[i];
				var parsedLogEntry = _parser.Parse(logEntry);
				destination[i].CopyFrom(parsedLogEntry);
			}
		}

		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			return _source.GetLogLineIndexOfOriginalLineIndex(originalLineIndex);
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
		}

		#endregion

		private sealed class ListenerProxy
			: ILogSourceListener
		{
			private readonly ILogSourceListener _listener;
			private readonly ILogSource _source;

			public ListenerProxy(ILogSource source, ILogSourceListener listener)
			{
				_source = source;
				_listener = listener;
			}


			#region Implementation of ILogSourceListener

			public void OnLogFileModified(ILogSource logSource, LogFileSection section)
			{
				_listener.OnLogFileModified(_source, section);
			}

			#endregion
		}
	}
}