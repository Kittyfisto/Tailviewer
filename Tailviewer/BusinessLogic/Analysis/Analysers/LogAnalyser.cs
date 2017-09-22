using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Core.LogTables;

namespace Tailviewer.BusinessLogic.Analysis.Analysers
{
	public abstract class LogAnalyser
		: ILogAnalyser
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected LogAnalyser()
		{
			_syncRoot = new object();
			_exceptions = new Queue<Exception>();
			_stopwatch = new Stopwatch();
		}

		private const int MaxExceptions = 10;

		private readonly object _syncRoot;
		private readonly Queue<Exception> _exceptions;
		private readonly Stopwatch _stopwatch;
		private TimeSpan _elapsed;

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			try
			{
				StartMeasure();
				OnLogFileModifiedInternal(logFile, section);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				AddException(e);
			}
			finally
			{
				StopMeasure();
			}
		}

		protected void StartMeasure()
		{
			_stopwatch.Start();
		}

		protected void StopMeasure()
		{
			_stopwatch.Stop();
			_elapsed += _stopwatch.Elapsed;
		}

		private void AddException(Exception exception)
		{
			lock (_syncRoot)
			{
				_exceptions.Enqueue(exception);
				if (_exceptions.Count > MaxExceptions)
					_exceptions.Dequeue();
			}
		}

		public void OnLogTableModified(ILogTable logTable, LogTableModification modification)
		{
			try
			{
				_stopwatch.Start();
				OnLogTableModifiedInternal(logTable, modification);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				AddException(e);
			}
			finally
			{
				_stopwatch.Stop();
				_elapsed += _stopwatch.Elapsed;
			}
		}

		public IReadOnlyList<Exception> UnexpectedExceptions
		{
			get
			{
				lock (_syncRoot)
				{
					return _exceptions.ToList();
				}
			}
		}

		public TimeSpan AnalysisTime => _elapsed;
		public abstract ILogAnalysisResult Result { get; }
		public abstract Percentage Progress { get; }

		public void Dispose()
		{
			try
			{
				DisposeInternal();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}
		
		protected abstract void OnLogFileModifiedInternal(ILogFile logFile, LogFileSection section);

		protected abstract void OnLogTableModifiedInternal(ILogTable logTable, LogTableModification modification);

		protected abstract void DisposeInternal();
	}
}