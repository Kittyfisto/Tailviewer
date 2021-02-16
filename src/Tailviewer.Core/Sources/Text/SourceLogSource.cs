using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
	internal sealed class SourceLogSource
		: AbstractLogSource
		, ILogSourceListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IServiceContainer _services;
		private readonly string _fileName;
		private readonly string _fullFilename;
		private readonly ITaskScheduler _taskScheduler;
		private readonly ITextLogFileParserPlugin2 _textLogFileParserPlugin;
		private readonly FileFormatDetector _detector;
		private readonly PropertiesBufferList _propertiesBuffer;
		private readonly ConcurrentPropertiesList _properties;
		private readonly TimeSpan _maximumWaitTime;
		private const int MaximumLineCount = 100000;

		#region Log Files

		private StreamingTextLogSource _streamingTextLogSource;
		private ILogSource _textLogSource;
		private MultiLineLogSource _multiLineLogSource;

		#endregion

		#region Processing

		private readonly ConcurrentQueue<KeyValuePair<ILogSource, LogFileSection>> _pendingSections;
		private ILogSource _logEntrySource;

		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="services"></param>
		/// <param name="fileName"></param>
		/// <param name="maximumWaitTime"></param>
		public SourceLogSource(IServiceContainer services, string fileName, TimeSpan maximumWaitTime)
			: base(services.Retrieve<ITaskScheduler>())
		{
			_services = services;
			_fileName = fileName;
			_fullFilename = Path.IsPathRooted(fileName)
				? fileName
				: Path.Combine(Directory.GetCurrentDirectory(), fileName);
			_maximumWaitTime = maximumWaitTime;

			_taskScheduler = services.Retrieve<ITaskScheduler>();
			_textLogFileParserPlugin = services.Retrieve<ITextLogFileParserPlugin2>();
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
			get { throw new NotImplementedException(); }
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
			var source = _logEntrySource;
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
			var source = _logEntrySource;
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
			_streamingTextLogSource?.TryDispose();

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
			var format = _detector.TryDetermineFormat(out var encoding);
			var formatChanged = _propertiesBuffer.SetValue(GeneralProperties.Format, format);
			var encodingChanged = _propertiesBuffer.SetValue(GeneralProperties.Encoding, encoding);

			if (_streamingTextLogSource == null ||
			    formatChanged ||
			    encodingChanged)
			{
				CreateStreamingTextLogFile(format, encoding);
			}
		}

		private void UpdateProperties()
		{
			_properties.CopyFrom(_propertiesBuffer);
		}

		private void ProcessPendingSections(out bool workDone)
		{
			workDone = false;
			while (_pendingSections.TryDequeue(out var pair))
			{
				// We may still have pending sections from a log file we've just removed as listener.
				// If that's the case, then throw away that section and go look for the next...
				if (!Equals(pair.Key, _logEntrySource))
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

		private void CreateStreamingTextLogFile(ILogFileFormat format, Encoding encoding)
		{
			_streamingTextLogSource?.TryDispose();
			if (format.IsText)
			{
				_streamingTextLogSource = new StreamingTextLogSource(_services, _fullFilename, encoding);
				CreateTextLogFile();
			}
			else
			{
				Log.WarnFormat("Log file {0} has been determined to be a binary file ({1}) - processing binary files is not implemented", _fullFilename, format);
			}
		}

		private void CreateTextLogFile()
		{
			_textLogSource?.TryDispose();
			_textLogSource = _textLogFileParserPlugin.CreateParser(_services, _streamingTextLogSource);

			if (_textLogSource.GetProperty(TextProperties.IsMultiline))
			{
				CreateMultiLineLogFile();

				_logEntrySource = _multiLineLogSource;
				_multiLineLogSource.AddListener(this, _maximumWaitTime, MaximumLineCount);
			}
			else
			{
				_logEntrySource = _textLogSource;
				_textLogSource.AddListener(this, _maximumWaitTime, MaximumLineCount);
			}
		}

		private void CreateMultiLineLogFile()
		{
			_multiLineLogSource?.TryDispose();
			_multiLineLogSource = new MultiLineLogSource(_taskScheduler, _textLogSource, _maximumWaitTime);
		}
	}
}
