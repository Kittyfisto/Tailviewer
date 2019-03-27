using System;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	/// Responsible for encapsulating failures of a wrapped <see cref="IDataSourceAnalyser"/>.
	/// </summary>
	public sealed class DataSourceAnalyserProxy
		: IDataSourceAnalyser
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IDataSourceAnalyserPlugin _plugin;
		private readonly AnalyserId _id;
		private readonly ILogFile _logFile;
		private readonly ILogAnalyserConfiguration _configuration;
		private readonly IDataSourceAnalyser _analyser;

		public DataSourceAnalyserProxy(IDataSourceAnalyserPlugin plugin, AnalyserId id, ILogFile logFile, ILogAnalyserConfiguration configuration)
		{
			_plugin = plugin;
			_id = id;
			_logFile = logFile;
			_configuration = configuration;

			_analyser = TryCreateAnalyser();
		}

		private IDataSourceAnalyser TryCreateAnalyser()
		{
			try
			{
				var analyser = _plugin.Create(_id, _logFile, _configuration);
				return analyser;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while trying to create analyser '{0}': {1}",
					_plugin.Id,
					e);
				return null;
			}
		}

		public void Dispose()
		{
			try
			{
				_analyser?.Dispose();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while trying to dispose of analyser '{0}': {1}",
					_plugin.Id,
					e);
			}
		}

		public AnalyserId Id => _id;

		public AnalyserPluginId AnalyserPluginId => _plugin.Id;

		public Percentage Progress
		{
			get
			{
				try
				{
					return _analyser?.Progress ?? Percentage.Zero;
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception while trying to query progress from analyser '{0}': {1}",
						_plugin.Id,
						e);
					return Percentage.Zero;
				}
			}
		}

		public ILogAnalysisResult Result
		{
			get
			{
				try
				{
					return _analyser?.Result;
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception while trying to query result from analyser '{0}': {1}",
						_plugin.Id,
						e);
					return null;
				}
			}
		}

		public bool IsFrozen => false;

		public ILogAnalyserConfiguration Configuration { get; set; }

		public void OnLogFileAdded(ILogFile logFile)
		{
			try
			{
				_analyser?.OnLogFileAdded(logFile);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while trying to add a log file to analyser '{0}': {1}",
					_plugin.Id,
					e);
			}
		}

		public void OnLogFileRemoved(ILogFile logFile)
		{
			try
			{
				_analyser?.OnLogFileRemoved(logFile);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while trying to remove a log file from analyser '{0}': {1}",
					_plugin.Id,
					e);
			}
		}
	}
}