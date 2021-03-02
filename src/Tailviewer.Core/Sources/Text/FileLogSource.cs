using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
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
		private readonly EncodingDetector _encodingDetector;
		private readonly FileFormatDetector _formatDetector;
		private readonly PropertiesBufferList _propertiesBuffer;
		private readonly ConcurrentPropertiesList _properties;
		private readonly TimeSpan _maximumWaitTime;
		private const int MaximumLineCount = 10000;
		private bool _isDisposed;

		#region Processing

		private readonly ConcurrentQueue<KeyValuePair<ILogSource, LogSourceModification>> _pendingSections;
		private readonly LogBufferArray _buffer;
		private IReadOnlyList<ILogSource> _logSources;
		private ILogSource _finalLogSource;
		private int _count;
		private FileFingerprint _lastFingerprint;
		private int _maxCharactersInLine;

		#endregion

		public FileLogSource(IServiceContainer services, string fileName)
			: this(services, fileName, TimeSpan.FromMilliseconds(100))
		{ }

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
			_encodingDetector = new EncodingDetector();
			_formatDetector = new FileFormatDetector(formatMatcher);

			_buffer = new LogBufferArray(MaximumLineCount, Core.Columns.RawContent);
			_pendingSections = new ConcurrentQueue<KeyValuePair<ILogSource, LogSourceModification>>();

			_propertiesBuffer = new PropertiesBufferList();
			_propertiesBuffer.SetValue(Core.Properties.Name, _fullFilename);

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
		                                  LogSourceQueryOptions queryOptions)
		{
			var source = _finalLogSource;
			if (source != null)
			{
				source.GetColumn(sourceIndices, column, destination, destinationIndex, queryOptions);
			}
			else
			{
				if (sourceIndices == null)
					throw new ArgumentNullException(nameof(sourceIndices));
				if (column == null)
					throw new ArgumentNullException(nameof(column));
				if (destination == null)
					throw new ArgumentNullException(nameof(destination));
				if (destinationIndex < 0)
					throw new ArgumentOutOfRangeException(nameof(destinationIndex));

				destination.Fill(column.DefaultValue, destinationIndex, sourceIndices.Count);
			}
		}

		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                                ILogBuffer destination,
		                                int destinationIndex,
		                                LogSourceQueryOptions queryOptions)
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
				_properties.Clear();
				// We do not want to dispose those sources from within the lock!
				// We don't know what they're doing and for how long they're doing it...
			}
			// https://github.com/Kittyfisto/Tailviewer/issues/282
			Dispose(logSources);

			base.DisposeAdditional();
		}

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			UpdateFormat();
			ProcessPendingSections(out bool workDone);
			UpdateProperties();

			if (workDone)
				return TimeSpan.Zero;

			return _maximumWaitTime;
		}

		private void UpdateFormat()
		{
			try
			{
				if (DetectFileChange(out var overwrittenEncoding))
				{
					if (!File.Exists(_fullFilename))
					{
						SetError(ErrorFlags.SourceDoesNotExist);
					}
					else
					{
						using (var stream = TryOpenRead(_fullFilename, out var error))
						{
							if (stream == null)
							{
								SetError(error);
							}
							else
							{
								var autoDetectedEncoding = _encodingDetector.TryFindEncoding(stream);
								var defaultEncoding = _services.TryRetrieve<Encoding>() ?? Encoding.Default;
								var format = _formatDetector.TryDetermineFormat(_fullFilename, stream, overwrittenEncoding ?? autoDetectedEncoding ?? defaultEncoding);
								var encoding = PickEncoding(overwrittenEncoding, format, autoDetectedEncoding, defaultEncoding);
								var formatChanged = _propertiesBuffer.SetValue(Core.Properties.Format, format);
								_propertiesBuffer.SetValue(TextProperties.AutoDetectedEncoding, autoDetectedEncoding);
								_propertiesBuffer.SetValue(TextProperties.ByteOrderMark, autoDetectedEncoding != null);
								_propertiesBuffer.SetValue(Core.Properties.EmptyReason, error);
								var encodingChanged = _propertiesBuffer.SetValue(TextProperties.Encoding, encoding);
								var currentLogSources = _logSources;

								if (currentLogSources == null || currentLogSources.Count == 0 ||
								    formatChanged || encodingChanged)
								{
									Dispose(currentLogSources);

									// Depending on the log file we're actually opening, the plugins we have installed, the final log source
									// that we expose here could be anything. It could be the raw log source which reads everything line by line,
									// but it also could be a plugin's ILogSource implementation which does all kinds of magic.
									var newLogSources = CreateAllLogSources(_services, _fullFilename, format, encoding, _maximumWaitTime);
									if (!StartListenTo(newLogSources))
									{
										Dispose(newLogSources);
									}
								}
							}
						}
					}
				}
			}
			catch (IOException e)
			{
				Log.DebugFormat("Caught exception while reading '{0}': {1}", _fullFilename, e);
			}
			catch (Exception e)
			{
				Log.DebugFormat("Caught unexpected exception while reading '{0}': {1}", _fullFilename, e);
			}
		}

		private static Stream TryOpenRead(string fileName, out ErrorFlags error)
		{
			try
			{
				error = ErrorFlags.None;
				return new FileStream(fileName, FileMode.Open, FileAccess.Read,
				                      FileShare.ReadWrite | FileShare.Delete);
			}
			catch (FileNotFoundException e)
			{
				error = ErrorFlags.SourceDoesNotExist;
				Log.Debug(e);
			}
			catch (DirectoryNotFoundException e)
			{
				error = ErrorFlags.SourceDoesNotExist;
				Log.Debug(e);
			}
			catch (UnauthorizedAccessException e)
			{
				error = ErrorFlags.SourceCannotBeAccessed;
				Log.Debug(e);
			}
			catch (IOException e)
			{
				error = ErrorFlags.SourceCannotBeAccessed;
				Log.Debug(e);
			}

			return null;
		}

		private void SetError(ErrorFlags error)
		{
			_propertiesBuffer.SetValue(Core.Properties.Format, null);
			_propertiesBuffer.SetValue(TextProperties.ByteOrderMark, null);
			_propertiesBuffer.SetValue(TextProperties.AutoDetectedEncoding, null);
			_propertiesBuffer.SetValue(TextProperties.Encoding, null);
			_propertiesBuffer.SetValue(TextProperties.MaxCharactersInLine, _maxCharactersInLine = 0);
			_propertiesBuffer.SetValue(Core.Properties.EmptyReason, error);
			_propertiesBuffer.SetValue(Core.Properties.Created, _lastFingerprint?.Created);
			_propertiesBuffer.SetValue(Core.Properties.LastModified, _lastFingerprint?.LastModified);
		}

		private bool DetectFileChange(out Encoding overwrittenEncoding)
		{
			overwrittenEncoding = null;
			if (!File.Exists(_fullFilename))
			{
				if (_lastFingerprint != null)
				{
					_lastFingerprint = null;
					Log.DebugFormat("File {0} no longer exists", _fullFilename);
					return true;
				}

				return false;
			}

			var currentFingerprint = FileFingerprint.FromFile(_fullFilename);
			if (!Equals(currentFingerprint, _lastFingerprint))
			{
				Log.DebugFormat("File {0} fingerprint changed from {1} to {2}", _fullFilename, _lastFingerprint, currentFingerprint);
				_lastFingerprint = currentFingerprint;
				return true;
			}

			overwrittenEncoding = _properties.GetValue(TextProperties.OverwrittenEncoding);
			var currentEncoding = _properties.GetValue(TextProperties.Encoding);
			if (!Equals(overwrittenEncoding, null) && !Equals(overwrittenEncoding, currentEncoding))
			{
				Log.DebugFormat("File {0} user chose an overwritten encoding {1} which differs from the current encoding {2}", _fullFilename, overwrittenEncoding, overwrittenEncoding);
				return true;
			}

			return false;
		}

		[Pure]
		private Encoding PickEncoding(Encoding overwrittenEncoding, ILogFileFormat format, Encoding detectedEncoding, Encoding defaultEncoding)
		{
			var formatEncoding = format?.Encoding;
			if (overwrittenEncoding != null)
			{
				if (formatEncoding != null)
					Log.DebugFormat("File {0} format specifies Encoding {1} but the user forced the encoding to be {2} instead",
					                _fullFilename,
					                formatEncoding.WebName,
					                overwrittenEncoding.WebName);

				return overwrittenEncoding;
			}

			if (formatEncoding != null)
			{
				if (detectedEncoding != null)
					Log.WarnFormat("File {0} has been detected to be encoded with {1}, but its format ({2}) says it's encoded with {3}, choosing the latter....",
					               _fullFilename,
					               detectedEncoding.WebName,
					               format,
					               formatEncoding.WebName);

				return formatEncoding;
			}

			if (detectedEncoding != null)
			{
				Log.DebugFormat("File {0}: Encoding was auto detected to be {1}", _fullFilename, detectedEncoding);
				return detectedEncoding;
			}

			Log.DebugFormat("File {0}: No encoding could be determined, falling back to {1}", _fullFilename, defaultEncoding);
			return defaultEncoding;
		}

		private void UpdateProperties()
		{
			if (_finalLogSource != null)
			{
				_finalLogSource.GetAllProperties(_propertiesBuffer.Except(TextProperties.AutoDetectedEncoding, Core.Properties.Format)); //< We don't want the log source to overwrite the encoding we just found out...
				_propertiesBuffer.SetValue(TextProperties.MaxCharactersInLine, _maxCharactersInLine);
			}
			else
			{
				_propertiesBuffer.SetValue(Core.Properties.PercentageProcessed, Percentage.HundredPercent);
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

				var modification = pair.Value;
				if (modification.IsReset())
				{
					Listeners.Reset();
					_count = 0;
					_maxCharactersInLine = 0;
				}
				else if (modification.IsRemoved(out var removedSection))
				{
					Listeners.Remove((int) removedSection.Index, removedSection.Count);
					_count = (int) removedSection.Index;
					// TODO: What about max width?
				}
				else if (modification.IsAppended(out var appendedSection))
				{
					Listeners.OnRead(appendedSection.LastIndex);
					_count = (int) (appendedSection.Index + appendedSection.Count);
					UpdateMaxWidth(appendedSection, pair.Key);
				}

				workDone = true;
			}

			if (!workDone)
				Listeners.OnRead(_count);
		}

		private void UpdateMaxWidth(LogSourceSection section, ILogSource logSource)
		{
			logSource.GetEntries(section, _buffer);
			for (int i = 0; i < section.Count; ++i)
			{
				var rawContent = _buffer[i].RawContent;
				if (rawContent == null)
					break;

				_maxCharactersInLine = Math.Max(_maxCharactersInLine, rawContent.Length);
			}
		}

		#endregion

		#region Implementation of ILogFileListener

		public void OnLogFileModified(ILogSource logSource, LogSourceModification modification)
		{
			_pendingSections.Enqueue(new KeyValuePair<ILogSource, LogSourceModification>(logSource, modification));
		}

		#endregion

		#region Log Source Creation

		private IReadOnlyList<ILogSource> CreateAllLogSources(IServiceContainer serviceContainer, string fullFilename, ILogFileFormat format, Encoding encoding, TimeSpan maximumWaitTime)
		{
			var newLogSources = new List<ILogSource>();
			var streamingLogSource = CreateTextLog(serviceContainer, fullFilename, format, encoding);
			if (streamingLogSource == null)
				return newLogSources;

			newLogSources.Add(streamingLogSource);

			var parsingLogSource = TryCreateParser(serviceContainer, streamingLogSource);
			if (parsingLogSource != null)
				newLogSources.Add(parsingLogSource);

			if (streamingLogSource.GetProperty(TextProperties.RequiresBuffer))
			{
				var bufferedLogSource = CreateBufferFor(serviceContainer, newLogSources.Last(), maximumWaitTime);
				newLogSources.Add(bufferedLogSource);
			}

			var propertyAdorner = new LogSourcePropertyAdorner(serviceContainer.Retrieve<ITaskScheduler>(), newLogSources.Last(), maximumWaitTime);
			newLogSources.Add(propertyAdorner);

			var columnAdorner = new LogSourceColumnAdorner(newLogSources.Last());
			newLogSources.Add(columnAdorner);

			var multiLineLogSource = TryCreateMultiLineLogFile(serviceContainer, newLogSources.Last(), maximumWaitTime);
			if (multiLineLogSource != null)
				newLogSources.Add(multiLineLogSource);

			return newLogSources;
		}

		private static ILogSource CreateTextLog(IServiceContainer serviceContainer, string fullFilename, ILogFileFormat format, Encoding encoding)
		{
			var fileLogSourceFactory = serviceContainer.Retrieve<IRawFileLogSourceFactory>();
			var textLogSource = fileLogSourceFactory.OpenRead(fullFilename, format, encoding);
			return textLogSource;
		}

		private static ILogSource TryCreateParser(IServiceContainer serviceContainer,
		                                          ILogSource source)
		{
			var logSourceParserPlugin = serviceContainer.Retrieve<ILogSourceParserPlugin>();
			var parsingLogSource = logSourceParserPlugin.CreateParser(serviceContainer, source);
			return parsingLogSource;
		}

		private ILogSource CreateBufferFor(IServiceContainer serviceContainer, ILogSource source, TimeSpan maximumWaitTime)
		{
			// Our streaming architecture isn't ready yet (the paged cache sucks, how when and what is cached suck, etc..
			// In order to get a release out, we will use the new streaming log source implementation, but we will
			// cache it all in memory anyways.
			return new FullyBufferedLogSource(serviceContainer.Retrieve<ITaskScheduler>(), source);

			/*var nonCachedColumns = new[] {StreamingTextLogSource.LineOffsetInBytes};
			return new PageBufferedLogSource(serviceContainer.Retrieve<ITaskScheduler>(),
			                             source,
			                             maximumWaitTime,
			                             nonCachedColumns);*/
		}

		private static ILogSource TryCreateMultiLineLogFile(IServiceContainer serviceContainer, ILogSource source, TimeSpan maximumWaitTime)
		{
			if (!source.GetProperty(TextProperties.AllowsMultiline))
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

		private bool StartListenTo(IReadOnlyList<ILogSource> logSource)
		{
			ILogSource finalLogSource;
			lock (_syncRoot)
			{
				if (_isDisposed)
				{
					return false;
				}

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
			return true;
		}
	}
}
