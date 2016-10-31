using System;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class SqLiteLogTable
		: ILogTable
	{
		private readonly SQLiteConnection _connection;
		private readonly ITaskScheduler _scheduler;
		private readonly IPeriodicTask _task;
		private readonly LogTableListenerCollection _listeners;
		private int _rowCount;
		private int _columnCount;

		public SqLiteLogTable(ITaskScheduler scheduler, string fileName)
		{
			var connectionString = string.Format("Data Source={0};Version=3;", fileName);
			_connection = new SQLiteConnection(connectionString);
			_connection.Open();

			_listeners = new LogTableListenerCollection();

			_scheduler = scheduler;
			_task = _scheduler.StartPeriodic(Update);
		}

		private TimeSpan Update()
		{
			throw new NotImplementedException();
		}

		public int RowCount
		{
			get { return _rowCount; }
		}

		public int ColumnCount
		{
			get { return _columnCount; }
		}

		public LogTableRow this[int index]
		{
			get { throw new System.NotImplementedException(); }
		}

		public void AddListener(ILogTableListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogTableListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public void Dispose()
		{
			_scheduler.StopPeriodic(_task);
		}
	}
}