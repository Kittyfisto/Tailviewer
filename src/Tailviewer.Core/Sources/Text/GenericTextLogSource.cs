using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;
using Tailviewer.Plugins;

namespace Tailviewer.Core.Sources.Text
{
	/// <summary>
	///   It's total BS that we create a fully proxied ILogSource for 99% of plugins which translate one entry at a time.
	///   Yes we need the implementation to translate stuff and request it from the source, but we don't need to listen
	///   to the source, nor do we need to re-fire those events from a dedicated thread...
	///   TODO: Convert this class to an accessor which doesn't implement any interface and simply contains the GetEntries
	///         and GetColumn call....
	/// </summary>
	internal sealed class GenericTextLogSource
		: AbstractLogSource
		, ILogSourceListener
	{
		private readonly ILogSource _source;
		private readonly IReadOnlyList<IColumnDescriptor> _columns;
		private readonly ILogEntryParser _parser;

		/// <summary>
		///    This object exists to hold all properties we want to eventually forward to the user.
		///    It is to be copied to <see cref="_properties"/> in *one* operation when we want the user to
		///    see its values.
		/// </summary>
		/// <remarks>
		///    This object should only be modified from the Task which executes RunOnce() and not from any other
		///    thread or there be demons.
		/// </remarks>
		private readonly PropertiesBufferList _localProperties;

		/// <summary>
		///    These are the properties the user gets to see. They are only updated when we deem it necessary.
		/// </summary>
		private readonly ConcurrentPropertiesList _properties;

		/// <summary>
		///    
		/// </summary>
		private readonly ConcurrentQueue<LogFileSection> _pendingChanges;

		private int _currentCount;

		public GenericTextLogSource(IServiceContainer services,
		                            ILogSource source,
		                            ILogEntryParser parser)
			: this(services.Retrieve<ITaskScheduler>(), source, parser, TimeSpan.FromMilliseconds(100))
		{ }

		public GenericTextLogSource(ITaskScheduler scheduler,
		                            ILogSource source,
		                            ILogEntryParser parser,
		                            TimeSpan maximumWaitTime)
			: base(scheduler)
		{
			_source = source ?? throw new ArgumentNullException(nameof(source));
			_columns = source.Columns.Concat(parser.Columns).Distinct().ToList();
			_parser = parser;
			_localProperties = new PropertiesBufferList();
			_properties = new ConcurrentPropertiesList();

			_pendingChanges = new ConcurrentQueue<LogFileSection>();
			_source.AddListener(this, maximumWaitTime, 10000);

			StartTask();
		}

		#region Overrides of AbstractLogSource

		public override IReadOnlyList<IColumnDescriptor> Columns
		{
			get
			{
				return _columns;
			}
		}

		public override IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get
			{
				return _properties.Properties;
			}
		}

		public override object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			return _properties.GetValue(property);
		}

		public override T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			return _properties.GetValue(property);
		}

		public override void SetProperty(IPropertyDescriptor property, object value)
		{
			_properties.SetValue(property, value);
		}

		public override void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_properties.SetValue(property, value);
		}

		public override void GetAllProperties(IPropertiesBuffer destination)
		{
			_properties.CopyAllValuesTo(destination);
		}

		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices,
		                                  IColumnDescriptor<T> column,
		                                  T[] destination,
		                                  int destinationIndex,
		                                  LogFileQueryOptions queryOptions)
		{
			GetEntries(sourceIndices, new SingleColumnLogBufferView<T>(column, destination, destinationIndex, sourceIndices.Count), 0, queryOptions);
		}

		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                                ILogBuffer destination,
		                                int destinationIndex,
		                                LogFileQueryOptions queryOptions)
		{
			if (destinationIndex != 0)
				throw new NotImplementedException();

			var rawBuffer = destination.Columns.Contains(LogColumns.RawContent)
				? (ILogBuffer)new LogBufferView(destination, LogColumns.RawContent)
				: (ILogBuffer)new LogBufferArray(sourceIndices.Count);
			_source.GetEntries(sourceIndices, rawBuffer, 0, queryOptions);

			for (int i = 0; i < rawBuffer.Count; ++i)
			{
				var logEntry = rawBuffer[i];
				var parsedLogEntry = _parser.Parse(logEntry);
				destination[i].CopyFrom(parsedLogEntry);
			}
		}

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			while (_pendingChanges.TryDequeue(out var nextChange))
			{
				if (nextChange.IsReset)
				{
					_currentCount = 0;
					Listeners.Reset();
				}
				else if (nextChange.IsInvalidate)
				{
					_currentCount = (int) nextChange.Index;
					Listeners.Invalidate((int) nextChange.Index, nextChange.Count);
				}
				else
				{
					_currentCount = (int) (nextChange.Index + nextChange.Count);
					Listeners.OnRead(_currentCount);
				}
			}

			// As is the nature of our listener collection, we have to
			// make sure to periodically call OnRead so the collection
			// can check if a particular wants to be notified now that
			// enough time has elapsed.
			Listeners.OnRead(_currentCount);

			_source.GetAllProperties(_localProperties);
			_properties.CopyFrom(_localProperties);

			return TimeSpan.Zero;
		}

		#endregion

		#region Implementation of ILogSourceListener

		public void OnLogFileModified(ILogSource logSource, LogFileSection section)
		{
			_pendingChanges.Enqueue(section);
		}

		#endregion
	}
}
