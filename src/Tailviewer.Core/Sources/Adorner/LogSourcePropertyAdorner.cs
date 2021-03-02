using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     This class is responsible for calculating the values for certain properties based on the data of an underlying
	///     <see cref="ILogSource" />.
	/// </summary>
	/// <remarks>
	///     This class can only adorn properties for which it knows how to calculate them.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogSourceDebuggerVisualization))]
	internal sealed class LogSourcePropertyAdorner
		: AbstractLogSource
		, ILogSourceListener
	{
		private static readonly IReadOnlyDictionary<IReadOnlyPropertyDescriptor, IColumnDescriptor> ColumnsByProperty;
		public static readonly IReadOnlyList<IReadOnlyPropertyDescriptor> AllAdornedProperties;

		private readonly ILogSource _source;
		private readonly TimeSpan _maximumWaitTime;
		private readonly IReadOnlyList<IReadOnlyPropertyDescriptor> _adornedProperties;
		private readonly IReadOnlyList<IColumnDescriptor> _requiredColumns;
		private readonly PropertiesBufferList _propertiesBuffer;
		private readonly ConcurrentPropertiesList _properties;
		private readonly ConcurrentQueue<LogSourceModification> _pendingSections;
		private readonly LogBufferArray _buffer;
		private int _count;

		static LogSourcePropertyAdorner()
		{
			ColumnsByProperty = new Dictionary<IReadOnlyPropertyDescriptor, IColumnDescriptor>
			{
				{Core.Properties.StartTimestamp, Core.Columns.Timestamp },
				{Core.Properties.EndTimestamp, Core.Columns.Timestamp },
				{Core.Properties.Duration, Core.Columns.Timestamp },

				{Core.Properties.TraceLogEntryCount, Core.Columns.LogLevel },
				{Core.Properties.DebugLogEntryCount, Core.Columns.LogLevel },
				{Core.Properties.InfoLogEntryCount, Core.Columns.LogLevel },
				{Core.Properties.WarningLogEntryCount, Core.Columns.LogLevel },
				{Core.Properties.ErrorLogEntryCount, Core.Columns.LogLevel },
				{Core.Properties.FatalLogEntryCount, Core.Columns.LogLevel },
				{Core.Properties.OtherLogEntryCount, Core.Columns.LogLevel },
			};

			AllAdornedProperties = ColumnsByProperty.Keys.ToList();
		}

		[Pure]
		private static IReadOnlyList<IColumnDescriptor> GetColumnsRequiredFor(IReadOnlyList<IReadOnlyPropertyDescriptor> adornedProperties)
		{
			var requiredColumns = new HashSet<IColumnDescriptor>
			{
				Core.Columns.Index
			};

			foreach (var property in adornedProperties)
			{
				if (!ColumnsByProperty.TryGetValue(property, out var column))
					throw new ArgumentException($"This log source doesn't support adorning '{property}'!");

				requiredColumns.Add(column);
			}

			return requiredColumns.ToList();
		}

		public LogSourcePropertyAdorner(ITaskScheduler scheduler, ILogSource source, TimeSpan maximumWaitTime)
			: this(scheduler, source, maximumWaitTime, AllAdornedProperties)
		{ }

		public LogSourcePropertyAdorner(ITaskScheduler scheduler, ILogSource source, TimeSpan maximumWaitTime, IReadOnlyList<IReadOnlyPropertyDescriptor> adornedProperties)
			: base(scheduler)
		{
			_source = source;
			_maximumWaitTime = maximumWaitTime;
			_adornedProperties = adornedProperties;
			_propertiesBuffer = new PropertiesBufferList(_adornedProperties);
			_properties = new ConcurrentPropertiesList(_adornedProperties);
			_pendingSections = new ConcurrentQueue<LogSourceModification>();
			_requiredColumns = GetColumnsRequiredFor(_adornedProperties);
			const int bufferSize = 1000;
			_buffer = new LogBufferArray(bufferSize, _requiredColumns);

			_source.AddListener(this, maximumWaitTime, bufferSize);
			StartTask();
		}

		#region Overrides of AbstractLogSource

		public override IReadOnlyList<IColumnDescriptor> Columns => _source.Columns;

		public override IReadOnlyList<IReadOnlyPropertyDescriptor> Properties => _properties.Properties;

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
			_source.SetProperty(property, value);
		}

		public override void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_source.SetProperty(property, value);
		}

		public override void GetAllProperties(IPropertiesBuffer destination)
		{
			_properties.CopyAllValuesTo(destination);
		}

		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices,
		                                  IColumnDescriptor<T> column,
		                                  T[] destination,
		                                  int destinationIndex,
		                                  LogSourceQueryOptions queryOptions)
		{
			_source.GetColumn(sourceIndices, column, destination, destinationIndex, queryOptions);
		}

		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                                ILogBuffer destination,
		                                int destinationIndex,
		                                LogSourceQueryOptions queryOptions)
		{
			_source.GetEntries(sourceIndices, destination, destinationIndex, queryOptions);
		}

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			var pendingSections = _pendingSections.DequeueAll();
			if (!Process(pendingSections))
			{
				SynchronizeProperties();
				Listeners.OnRead(_count);
			}

			return _maximumWaitTime;
		}

		#endregion

		private bool Process(IReadOnlyList<LogSourceModification> pendingModifications)
		{
			if (pendingModifications.Count == 0)
				return false;

			foreach (var modification in pendingModifications)
			{
				if (modification.IsReset())
				{
					_count = 0;
					Listeners.Reset();
					_propertiesBuffer.SetToDefault(_adornedProperties);
					SynchronizeProperties();
				}
				else if (modification.IsRemoved(out var removedSection))
				{
					_count = (int) removedSection.Index;
					SynchronizeProperties();
					Listeners.Remove((int) removedSection.Index, removedSection.Count);
				}
				else if (modification.IsAppended(out var appendedSection))
				{
					Process(appendedSection);
					_count += appendedSection.Count;
					SynchronizeProperties();
					Listeners.OnRead(_count);
				}
			}

			return true;
		}

		private void Process(LogSourceSection section)
		{
			DateTime? startTime = _propertiesBuffer.GetValue(Core.Properties.StartTimestamp);
			DateTime? endTime = _propertiesBuffer.GetValue(Core.Properties.EndTimestamp);
			int traceCount = _propertiesBuffer.GetValue(Core.Properties.TraceLogEntryCount);
			int debugCount = _propertiesBuffer.GetValue(Core.Properties.DebugLogEntryCount);
			int infoCount = _propertiesBuffer.GetValue(Core.Properties.InfoLogEntryCount);
			int warningCount = _propertiesBuffer.GetValue(Core.Properties.WarningLogEntryCount);
			int errorCount = _propertiesBuffer.GetValue(Core.Properties.ErrorLogEntryCount);
			int fatalCount = _propertiesBuffer.GetValue(Core.Properties.FatalLogEntryCount);
			int otherCount = _propertiesBuffer.GetValue(Core.Properties.OtherLogEntryCount);

			_source.GetEntries(section, _buffer, 0, LogSourceQueryOptions.Default);
			bool evaluateTimestamp = _requiredColumns.Contains(Core.Columns.Timestamp);
			bool evaluateLevel = _requiredColumns.Contains(Core.Columns.LogLevel);

			foreach (var entry in _buffer)
			{
				if (!entry.Index.IsValid)
					break;

				if (evaluateTimestamp)
				{
					var timestamp = entry.Timestamp;
					if (timestamp != null)
					{
						if (startTime == null)
							startTime = timestamp;
						else if (timestamp < startTime)
							startTime = timestamp;

						if (endTime == null)
							endTime = timestamp;
						else if (timestamp > endTime)
							endTime = timestamp;
					}
				}

				if (evaluateLevel)
				{
					var level = entry.LogLevel;
					switch (level)
					{
						case LevelFlags.Fatal:
							++fatalCount;
							break;
						case LevelFlags.Error:
							++errorCount;
							break;
						case LevelFlags.Warning:
							++warningCount;
							break;
						case LevelFlags.Info:
							++infoCount;
							break;
						case LevelFlags.Debug:
							++debugCount;
							break;
						case LevelFlags.Trace:
							++traceCount;
							break;
						case LevelFlags.Other:
							++otherCount;
							break;
					}
				}
			}

			if (evaluateTimestamp)
			{
				_propertiesBuffer.SetValue(Core.Properties.StartTimestamp, startTime);
				_propertiesBuffer.SetValue(Core.Properties.EndTimestamp, endTime);
				_propertiesBuffer.SetValue(Core.Properties.Duration, endTime - startTime);
			}

			if (evaluateLevel)
			{
				_propertiesBuffer.SetValue(Core.Properties.TraceLogEntryCount, traceCount);
				_propertiesBuffer.SetValue(Core.Properties.DebugLogEntryCount, debugCount);
				_propertiesBuffer.SetValue(Core.Properties.InfoLogEntryCount, infoCount);
				_propertiesBuffer.SetValue(Core.Properties.WarningLogEntryCount, warningCount);
				_propertiesBuffer.SetValue(Core.Properties.ErrorLogEntryCount, errorCount);
				_propertiesBuffer.SetValue(Core.Properties.FatalLogEntryCount, fatalCount);
				_propertiesBuffer.SetValue(Core.Properties.OtherLogEntryCount, otherCount);
			}
		}

		private void SynchronizeProperties()
		{
			_source.GetAllProperties(_propertiesBuffer.Except(_adornedProperties));

			var sourceProcessed = _propertiesBuffer.GetValue(Core.Properties.PercentageProcessed);
			var sourceCount = _propertiesBuffer.GetValue(Core.Properties.LogEntryCount);
			var ownProgress = sourceCount > 0
				? Percentage.Of(_count, sourceCount).Clamped()
				: Percentage.HundredPercent;
			var totalProgress = (sourceProcessed * ownProgress).Clamped();
			_propertiesBuffer.SetValue(Core.Properties.PercentageProcessed, totalProgress);
			_propertiesBuffer.SetValue(Core.Properties.LogEntryCount, _count);
			_properties.CopyFrom(_propertiesBuffer);
		}

		#region Implementation of ILogSourceListener

		public void OnLogFileModified(ILogSource logSource, LogSourceModification modification)
		{
			_pendingSections.Enqueue(modification);
		}

		#endregion
	}
}
