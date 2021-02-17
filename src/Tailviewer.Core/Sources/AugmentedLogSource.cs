using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;

namespace Tailviewer.Core.Sources
{
	/// <summary>
	///     This class is responsible for calculating the values for certain columns based on the data of an underlying
	///     <see cref="ILogSource" />.
	/// </summary>
	/// <remarks>
	///     Before this class existed, lots of <see cref="ILogSource"/> implementations contained the same piece of code
	///     implemented over and over. However this isn't necessary for lots of columns, namely:
	///      - <see cref="LogColumns.DeltaTime"/>
	///      - <see cref="LogColumns.ElapsedTime"/>
	/// </remarks>
	internal sealed class AugmentedLogSource
		: ILogSource
	{
		private readonly object _syncRoot;
		private readonly Dictionary<ILogSourceListener, ListenerProxy> _listeners;
		private ILogSource _source;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		public AugmentedLogSource(ILogSource source)
		{
			_syncRoot = new object();
			_source = source ?? throw new ArgumentNullException(nameof(source));
			_listeners = new Dictionary<ILogSourceListener, ListenerProxy>();
		}

		public IReadOnlyList<IColumnDescriptor> Columns => _source.Columns;

		public void AddListener(ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			// We need to make sure that whoever registers with us is getting OUR reference through
			// their listener, not the source we're wrapping (or they might discard events since they're
			// coming not from the source they subscribed to).
			var proxy = new ListenerProxy(this, listener);
			lock (_syncRoot)
			{
				_listeners.Add(listener, proxy);
			}

			_source?.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogSourceListener listener)
		{
			ListenerProxy proxy;
			lock (_syncRoot)
			{
				if (!_listeners.TryGetValue(listener, out proxy))
					return;
			}

			_source?.RemoveListener(proxy);
		}

		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get { return _source?.Properties ?? new IReadOnlyPropertyDescriptor[0]; }
		}

		public object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			var source = _source;
			if (source != null)
				return source.GetProperty(property);

			return property.DefaultValue;
		}

		public T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			var source = _source;
			if (source != null)
				return source.GetProperty(property);

			return property.DefaultValue;
		}

		public void SetProperty(IPropertyDescriptor property, object value)
		{
			_source?.SetProperty(property, value);
		}

		public void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_source?.SetProperty(property, value);
		}

		public void GetAllProperties(IPropertiesBuffer destination)
		{
			_source?.GetAllProperties(destination);
		}

		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices,
		                         IColumnDescriptor<T> column,
		                         T[] destination,
		                         int destinationIndex,
		                         LogFileQueryOptions queryOptions)
		{
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + sourceIndices.Count > destination.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

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

			// TODO: Should also fetch the index column so check where we accessed invalid regions?
			var rawBuffer = destination.Columns.Contains(LogColumns.Timestamp)
				? new LogBufferView(destination, LogColumns.Timestamp)
				: (ILogBuffer) new LogBufferArray(sourceIndices.Count, LogColumns.Timestamp);

			var source = _source;
			if (source != null)
			{
				source.GetEntries(sourceIndices, rawBuffer, destinationIndex: 0, queryOptions);

				for (var i = 0; i < rawBuffer.Count; ++i)
				{
					
				}
			}
			else
			{
				destination.FillDefault(destinationIndex, sourceIndices.Count);
			}
		}

		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			return _source?.GetLogLineIndexOfOriginalLineIndex(originalLineIndex) ?? LogLineIndex.Invalid;
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			_source?.Dispose();
			_source = null;
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