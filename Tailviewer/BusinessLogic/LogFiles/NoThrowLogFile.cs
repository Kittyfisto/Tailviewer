using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Metrolib;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     This class implements one of the layers of isolation between a third-party plugin
	///     and tailviewer: The ILogFile interface prohibits exceptions to occur, however we cannot
	///     rely on that to never happen. This implementation guarantuees the nothrow contract and also
	///     writes warnings to the log file.
	/// </summary>
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

		public bool Exists
		{
			get
			{
				try
				{
					return _logFile.Exists;
				}
				catch (Exception e)
				{
					BlameExceptionOnPlugin(e);
					return false;
				}
			}
		}

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

		public LogLineIndex GetOriginalIndexFrom(LogLineIndex index)
		{
			try
			{
				return _logFile.GetOriginalIndexFrom(index);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
				return new LogLineIndex();
			}
		}

		public void GetOriginalIndicesFrom(LogFileSection section, LogLineIndex[] originalIndices)
		{
			try
			{
				_logFile.GetOriginalIndicesFrom(section, originalIndices);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		public void GetOriginalIndicesFrom(IReadOnlyList<LogLineIndex> indices, LogLineIndex[] originalIndices)
		{
			try
			{
				_logFile.GetOriginalIndicesFrom(indices, originalIndices);
			}
			catch (Exception e)
			{
				BlameExceptionOnPlugin(e);
			}
		}

		private void BlameExceptionOnPlugin(Exception exception)
		{
			Log.WarnFormat("Plugin {0} threw an exception: {1}", _pluginName, exception);
		}
	}
}