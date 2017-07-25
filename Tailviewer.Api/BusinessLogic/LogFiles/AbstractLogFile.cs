using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using Metrolib;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     This abstract <see cref="ILogFile" /> implementation serves as a solid base class for most, if not all needs.
	/// </summary>
	/// <remarks>
	///     It is expected that any subclass performs ALL of its work inside the <see cref="RunOnce" /> method, which is called
	///     periodically via the given <see cref="ITaskScheduler" /> (as a bonus, you may easily test your implementation by
	///     using a <see cref="ManualTaskScheduler" /> inside your tests).
	/// </remarks>
	public abstract class AbstractLogFile
		: ILogFile
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly CancellationTokenSource _cancellationTokenSource;

		private readonly ITaskScheduler _scheduler;
		private volatile bool _endOfSourceReached;
		private IPeriodicTask _readTask;

		/// <summary>
		///     Initializes this log file.
		/// </summary>
		/// <param name="scheduler"></param>
		protected AbstractLogFile(ITaskScheduler scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));

			_scheduler = scheduler;
			_cancellationTokenSource = new CancellationTokenSource();
			Listeners = new LogFileListenerCollection(this);
		}

		/// <summary>
		///     The list of listeners which have been added to this <see cref="ILogFile" /> via
		///     <see cref="ILogFile.AddListener" /> and <see cref="ILogFile.RemoveListener" />.
		/// </summary>
		/// <remarks>
		///     It is expected that any subclass calls <see cref="LogFileListenerCollection.OnRead" /> when more lines have become
		///     available,
		///     <see cref="LogFileListenerCollection.Invalidate" /> when a certain region is no longer available and
		///     <see cref="LogFileListenerCollection.Reset" /> when the data source has become completely empty or has been
		///     deleted.
		/// </remarks>
		protected LogFileListenerCollection Listeners { get; }

		/// <summary>
		///     Whether or not <see cref="Dispose" /> has been called yet.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <inheritdoc />
		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			Listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		/// <inheritdoc />
		public void RemoveListener(ILogFileListener listener)
		{
			Listeners.RemoveListener(listener);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			try
			{
				DisposeAdditional();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
			_cancellationTokenSource.Cancel();
			IsDisposed = true;
			_scheduler.StopPeriodic(_readTask);
		}

		/// <inheritdoc />
		public virtual int OriginalCount => Count;

		/// <inheritdoc />
		public abstract int MaxCharactersPerLine { get; }

		/// <inheritdoc />
		public abstract bool Exists { get; }

		/// <inheritdoc />
		public virtual bool EndOfSourceReached => _endOfSourceReached;

		/// <inheritdoc />
		public abstract DateTime? StartTimestamp { get; }

		/// <inheritdoc />
		public abstract DateTime LastModified { get; }

		/// <inheritdoc />
		public abstract Size Size { get; }

		/// <inheritdoc />
		public abstract int Count { get; }

		/// <inheritdoc />
		public abstract void GetSection(LogFileSection section, LogLine[] dest);

		#region Index Translation

		/// <inheritdoc />
		public virtual LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			return originalLineIndex;
		}

		/// <inheritdoc />
		public virtual LogLineIndex GetOriginalIndexFrom(LogLineIndex index)
		{
			return index;
		}

		/// <inheritdoc />
		public virtual void GetOriginalIndicesFrom(LogFileSection section, LogLineIndex[] originalIndices)
		{
			if (originalIndices == null)
				throw new ArgumentNullException(nameof(originalIndices));
			if (section.Count > originalIndices.Length)
				throw new ArgumentOutOfRangeException(nameof(originalIndices));

			for (int i = 0; i < section.Count; ++i)
			{
				originalIndices[i] = section.Index + i;
			}
		}
		
		/// <inheritdoc />
		public virtual void GetOriginalIndicesFrom(IReadOnlyList<LogLineIndex> indices, LogLineIndex[] originalIndices)
		{
			// TODO: This implementation should actually behave differently, but I don't know which behaviour I actually want
			if (indices == null)
				throw new ArgumentNullException(nameof(indices));
			if (originalIndices == null)
				throw new ArgumentNullException(nameof(originalIndices));
			if (indices.Count > originalIndices.Length)
				throw new ArgumentOutOfRangeException(nameof(originalIndices));

			for (int i = 0; i < indices.Count; ++i)
			{
				originalIndices[i] = indices[i];
			}
		}

		#endregion

		/// <inheritdoc />
		public abstract LogLine GetLine(int index);

		/// <inheritdoc />
		public abstract double Progress { get; }

		/// <summary>
		///     This method may be implemented in order to dispose of any additional resources, but it doesn't need to be.
		/// </summary>
		protected virtual void DisposeAdditional()
		{
		}

		/// <summary>
		///     This method is called periodically
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		protected abstract TimeSpan RunOnce(CancellationToken token);

		private TimeSpan Run()
		{
			var token = _cancellationTokenSource.Token;
			return RunOnce(token);
		}

		/// <summary>
		/// </summary>
		protected void SetEndOfSourceReached()
		{
			// Now this line is very important:
			// Most tests expect that listeners have been notified
			// of all pending changes when the source enters the
			// "EndOfSourceReached" state. This would be true, if not
			// for listeners specifying a timespan that should ellapse between
			// calls to OnLogFileModified. The listener collection has
			// been notified, but the individual listeners may not be, because
			// neither the maximum line count, nor the maximum timespan has ellapsed.
			// Therefore we flush the collection to ensure that ALL listeners have been notified
			// of ALL changes (even if they didn't want them yet) before we enter the
			// EndOfSourceReached state.
			Listeners.Flush();
			_endOfSourceReached = true;
		}

		/// <summary>
		/// </summary>
		protected void ResetEndOfSourceReached()
		{
			_endOfSourceReached = false;
		}

		/// <summary>
		///     Any subclass MUST call this method in its constructor (perferably after the log file has been initialized,
		///     as after this call, <see cref="RunOnce" /> will be called.
		/// </summary>
		protected void StartTask()
		{
			_readTask = _scheduler.StartPeriodic(Run, ToString());
		}
	}
}