using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Tailviewer.Core.Buffers;

namespace Tailviewer.Core.Sources.Buffer
{
	[DebuggerTypeProxy(typeof(LogSourceDebuggerVisualization))]
	internal sealed class SimpleBufferedLogSource
		: AbstractLogSource
	{
		private readonly LogBufferList _buffer;

		public SimpleBufferedLogSource(ITaskScheduler taskScheduler,
		                               ILogSource source)
			: this(taskScheduler, source, source.Columns)
		{ }

		public SimpleBufferedLogSource(ITaskScheduler taskScheduler,
		                               ILogSource source,
		                               IReadOnlyList<IColumnDescriptor> bufferedColumns)
			: base(taskScheduler)
		{
			_buffer = new LogBufferList();

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

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}