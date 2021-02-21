using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Tailviewer.Core.Buffers;

namespace Tailviewer.Core.Sources.Buffer
{
	[DebuggerTypeProxy(typeof(LogSourceDebuggerVisualization))]
	internal sealed class SimpleBufferedLogSource
		: AbstractProxyLogSource
	{
		private readonly LogBufferList _buffer;
		private readonly object _syncRoot;

		public SimpleBufferedLogSource(ITaskScheduler taskScheduler,
		                               ILogSource source)
			: this(taskScheduler, source, source.Columns, TimeSpan.FromMilliseconds(100))
		{ }

		public SimpleBufferedLogSource(ITaskScheduler taskScheduler,
		                               ILogSource source,
		                               IReadOnlyList<IColumnDescriptor> bufferedColumns,
		                               TimeSpan maximumWaitTime)
			: base(taskScheduler, source, bufferedColumns, maximumWaitTime)
		{
			_buffer = new LogBufferList(bufferedColumns);
			_syncRoot = new object();

			StartTask();
		}

		#region Overrides of AbstractLogSource

		public override IReadOnlyList<IColumnDescriptor> Columns
		{
			get { throw new NotImplementedException(); }
		}

		public override IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get { throw new NotImplementedException(); }
		}

		public override object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			throw new NotImplementedException();
		}

		public override T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			throw new NotImplementedException();
		}

		public override void SetProperty(IPropertyDescriptor property, object value)
		{
			throw new NotImplementedException();
		}

		public override void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			throw new NotImplementedException();
		}

		public override void GetAllProperties(IPropertiesBuffer destination)
		{
			throw new NotImplementedException();
		}

		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices,
		                                  IColumnDescriptor<T> column,
		                                  T[] destination,
		                                  int destinationIndex,
		                                  LogSourceQueryOptions queryOptions)
		{
			throw new NotImplementedException();
		}

		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                                ILogBuffer destination,
		                                int destinationIndex,
		                                LogSourceQueryOptions queryOptions)
		{
			throw new NotImplementedException();
		}

		protected override void OnReset()
		{
			lock (_syncRoot)
			{
				_buffer.Clear();
			}
		}

		protected override void OnInvalidate(int totalCount)
		{
			lock (_syncRoot)
			{
				_buffer.RemoveRange(totalCount, _buffer.Count - totalCount);
			}
		}

		protected override void OnAdd(LogFileSection section, IReadOnlyLogBuffer data, int totalLogEntryCount)
		{
			lock (_syncRoot)
			{
				_buffer.AddRange(data, section.Count);
			}
		}

		protected override void NothingToProcess()
		{
			
		}

		#endregion
	}
}