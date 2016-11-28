using System.Threading.Tasks;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogTables;

namespace TablePlayground
{
	public partial class MainWindow
	{
		private readonly ITaskScheduler _scheduler;
		private readonly LogDataCache _cache;
		private readonly SQLiteLogTable _table;

		public MainWindow()
		{
			InitializeComponent();

			_scheduler = new DefaultTaskScheduler();
			_cache = new LogDataCache();
			_table = new SQLiteLogTable(_scheduler, _cache, @"..\Live\SQLiteLogger.db");

			TableView.LogTable = _table;
		}
	}
}
