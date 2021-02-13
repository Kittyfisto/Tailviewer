using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     This class implements one of the layers of isolation between a third-party plugin
	///     and tailviewer: The ILogFile interface prohibits exceptions to occur, however we cannot
	///     rely on that to never happen. This implementation guarantuees the nothrow contract and also
	///     writes warnings to the log file.
	/// </summary>
	/// <remarks>
	///    Plugin authors are deliberately prevented from instantiating this type directly because it's constructor signature may change
	///    over time. In order to create an instance of this type, simply call <see cref="IServiceContainer.CreateNoThrowLogFile"/>
	///    who's signature is guaranteed to never change.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogFileView))]
	internal sealed class NoThrowLogFile
		: ILogFile
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFile _logFile;
		private readonly string _pluginName;

		/// <summary>
		/// </summary>
		/// <remarks>
		///     Takes ownership of the given <paramref name="logFile" /> and disposes it when disposed of.
		/// </remarks>
		/// <param name="logFile">The log file to represent</param>
		/// <param name="pluginName">
		///     The plugin from which <paramref name="logFile" /> has been created, is used to blame that specific
		///     plugin the log file output
		/// </param>
		public NoThrowLogFile(ILogFile logFile, string pluginName)
		{
			if (logFile == null)
				throw new ArgumentNullException(nameof(logFile));
			if (pluginName == null)
				throw new ArgumentNullException(nameof(pluginName));

			_logFile = logFile;
			_pluginName = pluginName;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			try
			{
				_logFile.Dispose();
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public bool EndOfSourceReached
		{
			get
			{
				try
				{
					return _logFile.EndOfSourceReached;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return true;
				}
			}
		}

		/// <inheritdoc />
		public int Count
		{
			get
			{
				try
				{
					return _logFile.Count;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return 0;
				}
			}
		}

		/// <inheritdoc />
		public int OriginalCount
		{
			get
			{
				try
				{
					return _logFile.OriginalCount;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return 0;
				}
			}
		}

		/// <inheritdoc />
		public int MaxCharactersPerLine
		{
			get
			{
				try
				{
					return _logFile.MaxCharactersPerLine;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return 0;
				}
			}
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumnDescriptor> Columns
		{
			get
			{
				try
				{
					return _logFile.Columns;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return new ILogFileColumnDescriptor[0];
				}
			}
		}

		/// <inheritdoc />
		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			try
			{
				_logFile.AddListener(listener, maximumWaitTime, maximumLineCount);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public void RemoveListener(ILogFileListener listener)
		{
			try
			{
				_logFile.RemoveListener(listener);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFilePropertyDescriptor> Properties
		{
			get
			{
				try
				{
					return _logFile.Properties;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return new ILogFilePropertyDescriptor[0];
				}
			}
		}

		/// <inheritdoc />
		public object GetValue(ILogFilePropertyDescriptor propertyDescriptor)
		{
			try
			{
				return _logFile.GetValue(propertyDescriptor);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
				return propertyDescriptor.DefaultValue;
			}
		}

		/// <inheritdoc />
		public T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			try
			{
				return _logFile.GetValue(propertyDescriptor);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
				return propertyDescriptor.DefaultValue;
			}
		}

		/// <inheritdoc />
		public void GetValues(ILogFileProperties properties)
		{
			try
			{
				_logFile.GetValues(properties);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
				throw;
			}
		}

		/// <inheritdoc />
		public void GetColumn<T>(LogFileSection sourceSection, ILogFileColumnDescriptor<T> column, T[] destination, int destinationIndex)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + sourceSection.Count > destination.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			try
			{
				_logFile.GetColumn(sourceSection, column, destination, destinationIndex);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, ILogFileColumnDescriptor<T> column, T[] destination, int destinationIndex)
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

			try
			{
				_logFile.GetColumn(sourceIndices, column, destination, destinationIndex);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public void GetEntries(LogFileSection sourceSection, ILogEntries destination, int destinationIndex)
		{
			try
			{
				_logFile.GetEntries(sourceSection, destination, destinationIndex);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination, int destinationIndex)
		{
			try
			{
				_logFile.GetEntries(sourceIndices, destination, destinationIndex);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public double Progress
		{
			get
			{
				try
				{
					return _logFile.Progress;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return 1;
				}
			}
		}

		/// <inheritdoc />
		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			try
			{
				return _logFile.GetLogLineIndexOfOriginalLineIndex(originalLineIndex);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
				return new LogLineIndex();
			}
		}

		private void BlameExceptionOnPlugin(Exception exception)
		{
			Log.WarnFormat("Plugin {0} threw an exception: {1}", _pluginName, exception);
		}
	}
}