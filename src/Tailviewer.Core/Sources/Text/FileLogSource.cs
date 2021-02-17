using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Tailviewer.Core.Properties;
using Tailviewer.Plugins;

namespace Tailviewer.Core.Sources.Text
{
	/// <summary>
	///    Responsible for interpreting any file as a log file and exposing its data as log entries.
	/// </summary>
	/// <remarks>
	///    This <see cref="ILogSource"/> implementation aggregates most other implementations to:
	///    - Have a single log file implementation for everything (previously, this business logic was part of an IDataSource, but we don't want that anymore)
	///    - Reloading a log file from scratch again
	///      - When the encoding has been changed by the user
	///      - When a plugin has been added / removed
	///      - When the format detector changes its mind and is now even more sure that the format is actually is
	///    - Share functionality between different formats (streaming text files into memory shouldn't be done by the same class which is trying to make sense of the data)
	/// </remarks>
	internal sealed class FileLogSource
		: AbstractLogSource
		, ILogSourceListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		private readonly object _syncRoot;
		private readonly IServiceContainer _services;
		private readonly string _fullFilename;
		private readonly FileFormatDetector _detector;
		private readonly PropertiesBufferList _propertiesBuffer;
		private readonly ConcurrentPropertiesList _properties;
		private readonly TimeSpan _maximumWaitTime;
		private const int MaximumLineCount = 100000;
		private bool _isDisposed;

		#region Processing

		private readonly ConcurrentQueue<KeyValuePair<ILogSource, LogFileSection>> _pendingSections;
		private IReadOnlyList<ILogSource> _logSources;
		private ILogSource _finalLogSource;

		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="services"></param>
		/// <param name="fileName"></param>
		/// <param name="maximumWaitTime"></param>
		public FileLogSource(IServiceContainer services, string fileName, TimeSpan maximumWaitTime)
			: base(services.Retrieve<ITaskScheduler>())
		{
			_syncRoot = new object();
			_services = services;
			_fullFilename = Path.IsPathRooted(fileName)
				? fileName
				: Path.Combine(Directory.GetCurrentDirectory(), fileName);
			_maximumWaitTime = maximumWaitTime;

			var formatMatcher = services.Retrieve<ILogFileFormatMatcher>();
			// TODO: Fetch default encoding from settings
			_detector = new FileFormatDetector(formatMatcher, _fullFilename, Encoding.Default);

			_pendingSections = new ConcurrentQueue<KeyValuePair<ILogSource, LogFileSection>>();

			_propertiesBuffer = new PropertiesBufferList();
			_propertiesBuffer.SetValue(GeneralProperties.Name, _fullFilename);

			_properties = new ConcurrentPropertiesList();

			StartTask();
		}

		#region Overrides of AbstractLogFile

		public override IReadOnlyList<IColumnDescriptor> Columns
		{
			get
			{
				return _finalLogSource?.Columns ?? new IColumnDescriptor[0];
			}
		}

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
			var source = _finalLogSource;
			if (source != null)
			{
				source.GetColumn(sourceIndices, column, destination, destinationIndex, queryOptions);
			}
			else
			{
				// TODO: Fill
			}
		}

		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                                ILogBuffer destination,
		                                int destinationIndex,
		                                LogFileQueryOptions queryOptions)
		{
			var source = _finalLogSource;
			if (source != null)
			{
				source.GetEntries(sourceIndices, destination, destinationIndex, queryOptions);
			}
			else
			{
				destination.FillDefault(destinationIndex, sourceIndices.Count);
			}
		}

		protected override void DisposeAdditional()
		{
			IReadOnlyList<ILogSource> logSources;
			lock (_syncRoot)
			{
				_isDisposed = true;
				logSources = _logSources;
				_logSources = null;
				_properties.SetValue(GeneralProperties.LogEntryCount, 0);
				// We do not want to dispose those sources from within the lock!
				// We don't know what they're doing and for how long they're doing it...
			}
			Dispose(logSources);

			base.DisposeAdditional();
		}

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			UpdateFormat();
			UpdateProperties();
			ProcessPendingSections(out bool workDone);

			if (workDone)
				return TimeSpan.Zero;

			return _maximumWaitTime;
		}

		private void UpdateFormat()
		{
			// The most important thing about the following method is that we try to get away with holding a lock for the smallest amount of time.
			// It's especially important that we don't hold a lock while we're calling plugins and such because that is likely to cause
			// problems when we're being disposed of. Therefore we first obtain a copy of the relevant values here, create the log source list
			// as needed and *then* check to see if it actually is needed or if we have been disposed of at the last possible moment.

			var format = _detector.TryDetermineFormat(out var encoding);
			var formatChanged = _propertiesBuffer.SetValue(GeneralProperties.Format, format);
			var encodingChanged = _propertiesBuffer.SetValue(GeneralProperties.Encoding, encoding);
			var currentLogSources = _logSources;

			if (currentLogSources == null || currentLogSources.Count == 0 ||
			    formatChanged || encodingChanged)
			{
				Dispose(currentLogSources);

				// Depending on the log file we're actually opening, the plugins we have installed, the final log source
				// that we expose here could be anything. It could be the raw log source which reads everything line by line,
				// but it also could be a plugin's ILogSource implementation which does all kinds of magic.
				var newLogSources = CreateAllLogSources(_services, _fullFilename, format, encoding, _maximumWaitTime);
				StartListenTo(newLogSources);
			}
		}

		private void UpdateProperties()
		{
			if (_finalLogSource != null)
			{
				_finalLogSource.GetAllProperties(_propertiesBuffer.Except(GeneralProperties.Encoding, GeneralProperties.Format)); //< We don't want the log source to overwrite the encoding we just found out...
			}
			else
			{
				_propertiesBuffer.SetValue(GeneralProperties.PercentageProcessed, Percentage.HundredPercent);
			}

			_properties.CopyFrom(_propertiesBuffer);
		}

		private void ProcessPendingSections(out bool workDone)
		{
			workDone = false;
			while (_pendingSections.TryDequeue(out var pair))
			{
				// We may still have pending sections from a log file we've just removed as listener.
				// If that's the case, then throw away that section and go look for the next...
				if (!Equals(pair.Key, _finalLogSource))
					continue;

				var section = pair.Value;
				if (section.IsReset)
				{
					Listeners.Reset();
				}
				else if (section.IsInvalidate)
				{
					Listeners.Invalidate((int) section.Index, section.Count);
				}
				else
				{
					Listeners.OnRead(section.LastIndex);
				}

				workDone = true;
			}
		}

		#endregion

		#region Implementation of ILogFileListener

		public void OnLogFileModified(ILogSource logSource, LogFileSection section)
		{
			_pendingSections.Enqueue(new KeyValuePair<ILogSource, LogFileSection>(logSource, section));
		}

		#endregion

		#region Log Source Creation

		private IReadOnlyList<ILogSource> CreateAllLogSources(IServiceContainer serviceContainer, string fullFilename, ILogFileFormat format, Encoding encoding, TimeSpan maximumWaitTime)
		{
			var newLogSources = new List<ILogSource>();
			var streamingLogSource = TryCreateStreamingTextLogFile(serviceContainer, fullFilename, format, encoding);
			if (streamingLogSource != null)
				newLogSources.Add(streamingLogSource);

			var parsingLogSource = CreateLogSourceParser(serviceContainer, streamingLogSource);
			if (parsingLogSource != null)
				newLogSources.Add(parsingLogSource);

			var multiLineLogSource = TryCreateMultiLineLogFile(serviceContainer, newLogSources.Last(), maximumWaitTime);
			if (multiLineLogSource != null)
				newLogSources.Add(multiLineLogSource);

			return newLogSources;
		}

		private static ILogSource TryCreateStreamingTextLogFile(IServiceContainer serviceContainer, string fullFilename, ILogFileFormat format, Encoding encoding)
		{
			var fileLogSourceFactory = serviceContainer.Retrieve<IFileLogSourceFactory>();
			var textLogSource = fileLogSourceFactory.OpenRead(fullFilename, format, encoding);
			return textLogSource;
		}

		private static ILogSource CreateLogSourceParser(IServiceContainer serviceContainer, ILogSource source)
		{
			var logSourceParserPlugin = serviceContainer.Retrieve<ILogSourceParserPlugin>();
			var parsingLogSource = logSourceParserPlugin.CreateParser(serviceContainer, source);
			return parsingLogSource;
		}

		private static ILogSource TryCreateMultiLineLogFile(IServiceContainer serviceContainer, ILogSource source, TimeSpan maximumWaitTime)
		{
			if (!source.GetProperty(TextProperties.IsMultiline))
				return null;

			var multiLineLogSource = new MultiLineLogSource(serviceContainer.Retrieve<ITaskScheduler>(), source, maximumWaitTime);
			return multiLineLogSource;
		}

		#endregion

		private void Dispose(IReadOnlyList<ILogSource> logSources)
		{
			if (logSources == null)
				return;

			foreach (var logSource in logSources)
			{
				logSource?.TryDispose();
			}
		}

		private void StartListenTo(IReadOnlyList<ILogSource> logSource)
		{
			ILogSource finalLogSource;
			lock (_syncRoot)
			{
				if (_isDisposed)
					return;

				if (logSource != null && logSource.Count > 0)
				{
					_logSources = logSource;
					_finalLogSource = finalLogSource = logSource.Last();
				}
				else
				{
					_logSources = null;
					_finalLogSource = finalLogSource = null;
				}

				// We do not want to call AddListener from within the lock!
			}

			finalLogSource?.AddListener(this, _maximumWaitTime, MaximumLineCount);
		}
	}
}
