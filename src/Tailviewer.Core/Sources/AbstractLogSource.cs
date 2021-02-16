using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;

namespace Tailviewer.Core.Sources
{
	/// <summary>
	///     This abstract <see cref="ILogSource" /> implementation serves as a solid base class for most, if not all needs.
	/// </summary>
	/// <remarks>
	///     It is expected that any subclass performs ALL of its work inside the <see cref="RunOnce" /> method, which is called
	///     periodically via the given <see cref="ITaskScheduler" /> (as a bonus, you may easily test your implementation by
	///     using a <see cref="ManualTaskScheduler" /> inside your tests).
	/// </remarks>
	public abstract class AbstractLogSource
		: ILogSource
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly CancellationTokenSource _cancellationTokenSource;

		private readonly ITaskScheduler _scheduler;
		private IPeriodicTask _readTask;

		/// <summary>
		///     Initializes this log file.
		/// </summary>
		/// <param name="scheduler"></param>
		protected AbstractLogSource(ITaskScheduler scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));

			_scheduler = scheduler;
			_cancellationTokenSource = new CancellationTokenSource();
			Listeners = new LogSourceListenerCollection(this);
		}

		/// <summary>
		///     The list of listeners which have been added to this <see cref="ILogSource" /> via
		///     <see cref="ILogSource.AddListener" /> and <see cref="ILogSource.RemoveListener" />.
		/// </summary>
		/// <remarks>
		///     It is expected that any subclass calls <see cref="LogSourceListenerCollection.OnRead" /> when more lines have become
		///     available,
		///     <see cref="LogSourceListenerCollection.Invalidate" /> when a certain region is no longer available and
		///     <see cref="LogSourceListenerCollection.Reset" /> when the data source has become completely empty or has been
		///     deleted.
		/// </remarks>
		protected LogSourceListenerCollection Listeners { get; }

		/// <summary>
		///     Whether or not <see cref="Dispose" /> has been called yet.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <inheritdoc />
		public abstract IReadOnlyList<IColumnDescriptor> Columns { get; }

		/// <inheritdoc />
		public void AddListener(ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			Listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		/// <inheritdoc />
		public void RemoveListener(ILogSourceListener listener)
		{
			Listeners.RemoveListener(listener);
		}

		/// <inheritdoc />
		public abstract IReadOnlyList<IReadOnlyPropertyDescriptor> Properties { get; }

		/// <inheritdoc />
		public abstract object GetProperty(IReadOnlyPropertyDescriptor property);

		/// <inheritdoc />
		public abstract T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property);

		/// <inheritdoc />
		public abstract void SetProperty(IPropertyDescriptor property, object value);

		/// <inheritdoc />
		public abstract void SetProperty<T>(IPropertyDescriptor<T> property, T value);

		/// <inheritdoc />
		public abstract void GetAllProperties(IPropertiesBuffer destination);

		/// <inheritdoc />
		public void Dispose()
		{
			// https://github.com/Kittyfisto/Tailviewer/issues/282
			Listeners.Clear();

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
		public abstract void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogFileQueryOptions queryOptions);

		/// <inheritdoc />
		public abstract void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex, LogFileQueryOptions queryOptions);

		#region Index Translation

		/// <inheritdoc />
		public virtual LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			return originalLineIndex;
		}

		#endregion

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
		///     Any subclass MUST call this method in its constructor (preferably after the log file has been initialized,
		///     as after this call, <see cref="RunOnce" /> will be called.
		/// </summary>
		protected void StartTask()
		{
			_readTask = _scheduler.StartPeriodic(Run, ToString());
		}
	}
}