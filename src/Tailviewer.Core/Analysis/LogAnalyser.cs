using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     Base class for <see cref="ILogAnalyser" /> implementations.
	///     Suitable for most implementations.
	/// </summary>
	public abstract class LogAnalyser
		: ILogAnalyser
	{
		private const int MaxExceptions = 10;
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly Queue<Exception> _exceptions;
		private readonly Stopwatch _stopwatch;

		private readonly object _syncRoot;

		/// <summary>
		///     Initializes this analyser.
		/// </summary>
		protected LogAnalyser()
		{
			_syncRoot = new object();
			_exceptions = new Queue<Exception>();
			_stopwatch = new Stopwatch();
		}

		/// <inheritdoc />
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

		/// <inheritdoc />
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

		/// <inheritdoc />
		public TimeSpan AnalysisTime { get; private set; }

		/// <inheritdoc />
		public abstract ILogAnalysisResult Result { get; }

		/// <inheritdoc />
		public abstract Percentage Progress { get; }

		/// <inheritdoc />
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

		/// <summary>
		///     Invoke this method right before work is being done.
		/// </summary>
		protected void StartMeasure()
		{
			_stopwatch.Start();
		}

		/// <summary>
		///     Invoke this method right before work no longer needs to be performed.
		/// </summary>
		protected void StopMeasure()
		{
			_stopwatch.Stop();
			AnalysisTime += _stopwatch.Elapsed;
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

		/// <summary>
		///     This method is called when the log file is modified.
		/// </summary>
		/// <param name="logFile"></param>
		/// <param name="section"></param>
		protected abstract void OnLogFileModifiedInternal(ILogFile logFile, LogFileSection section);

		/// <summary>
		///     This method is called when this object is being disposed of.
		/// </summary>
		protected abstract void DisposeInternal();
	}
}