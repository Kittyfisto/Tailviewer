using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using log4net;
using Metrolib;
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
	[DebuggerTypeProxy(typeof(LogFileView))]
	public sealed class NoThrowLogFile
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
		public DateTime? StartTimestamp
		{
			get
			{
				try
				{
					return _logFile.StartTimestamp;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return null;
				}
			}
		}

		/// <inheritdoc />
		public DateTime LastModified
		{
			get
			{
				try
				{
					return _logFile.LastModified;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return DateTime.MinValue;
				}
			}
		}

		/// <inheritdoc />
		public DateTime Created
		{
			get
			{
				try
				{
					return _logFile.Created;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return DateTime.MinValue;
				}
			}
		}

		/// <inheritdoc />
		public Size Size
		{
			get
			{
				try
				{
					return _logFile.Size;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return Size.Zero;
				}
			}
		}

		/// <inheritdoc />
		public ErrorFlags Error
		{
			get
			{
				try
				{
					return _logFile.Error;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return ErrorFlags.None;
				}
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
		public IReadOnlyList<ILogFileColumn> Columns
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
					return new ILogFileColumn[0];
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
		public void GetColumn<T>(LogFileSection section, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + section.Count > buffer.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			try
			{
				_logFile.GetColumn(section, column, buffer, destinationIndex);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			if (indices == null)
				throw new ArgumentNullException(nameof(indices));
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + indices.Count > buffer.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			try
			{
				_logFile.GetColumn(indices, column, buffer, destinationIndex);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public void GetEntries(LogFileSection section, ILogEntries buffer, int destinationIndex)
		{
			try
			{
				_logFile.GetEntries(section, buffer, destinationIndex);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public void GetEntries(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex)
		{
			try
			{
				_logFile.GetEntries(indices, buffer, destinationIndex);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			try
			{
				_logFile.GetSection(section, dest);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		/// <inheritdoc />
		public LogLine GetLine(int index)
		{
			try
			{
				return _logFile.GetLine(index);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
				return new LogLine();
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