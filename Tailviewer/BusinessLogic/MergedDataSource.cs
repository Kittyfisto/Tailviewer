using System.Collections.Generic;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic
{
	internal sealed class MergedDataSource
		: AbstractDataSource
	{
		private readonly MergedLogFile _logFile;
		private readonly List<IDataSource> _dataSources;

		public MergedDataSource(DataSource settings) : base(settings)
		{
			_logFile = new MergedLogFile();
			_dataSources = new List<IDataSource>();
		}

		public override ILogFile LogFile
		{
			get { return _logFile; }
		}

		public void Add(IDataSource dataSource)
		{
			_dataSources.Add(dataSource);
		}

		public void Remove(IDataSource dataSource)
		{
			_dataSources.Remove(dataSource);
		}
	}
}