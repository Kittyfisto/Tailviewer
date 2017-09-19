using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for holding one or more <see cref="IDataSourceAnalyser" />:
	///     They can be <see cref="Add" />ed as well as <see cref="Remove" />d.
	/// </summary>
	public sealed class AnalyserGroup
		: IAnalyserGroup
			, IDisposable
	{
		private readonly List<DataSourceAnalyser> _analysers;
		private readonly IAnalysisEngine _analysisEngine;
		private readonly IDataSource _dataSource;
		private readonly object _syncRoot;

		public AnalyserGroup(IDataSource dataSource, IAnalysisEngine analysisEngine)
		{
			_dataSource = dataSource;
			_analysisEngine = analysisEngine;
			_analysers = new List<DataSourceAnalyser>();
			_syncRoot = new object();
		}

		public IEnumerable<IDataSourceAnalyser> Analysers
		{
			get
			{
				lock (_syncRoot)
				{
					return _analysers.ToList();
				}
			}
		}

		public bool IsFrozen => false;

		/// <summary>
		/// </summary>
		/// <param name="analyserId"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public IDataSourceAnalyser Add(LogAnalyserFactoryId analyserId, ILogAnalyserConfiguration configuration)
		{
			var analyser = new DataSourceAnalyser(_dataSource, _analysisEngine, analyserId);
			try
			{
				analyser.Configuration = configuration;
				lock (_syncRoot)
				{
					_analysers.Add(analyser);
				}

				return analyser;
			}
			catch (Exception)
			{
				analyser.Dispose();
				throw;
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="analyser"></param>
		public void Remove(IDataSourceAnalyser analyser)
		{
			var tmp = analyser as DataSourceAnalyser;
			lock (_syncRoot)
			{
				if (_analysers.Remove(tmp))
					tmp?.Dispose();
			}
		}

		public void Dispose()
		{
			lock (_syncRoot)
			{
				foreach (var analyser in _analysers)
					analyser.Dispose();
			}
		}

		/// <summary>
		///     Creates a snapshot of this group's analysers.
		/// </summary>
		/// <returns></returns>
		public AnalyserGroupSnapshot CreateSnapshot()
		{
			lock (_syncRoot)
			{
				var analysers = new List<DataSourceAnalyserSnapshot>(_analysers.Count);
				foreach (var analyser in _analysers)
					analysers.Add(analyser.CreateSnapshot());
				return new AnalyserGroupSnapshot(analysers);
			}
		}
	}
}