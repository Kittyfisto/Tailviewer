using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for holding one or more <see cref="IDataSourceAnalyser" />:
	///     They can be <see cref="Add" />ed as well as <see cref="Remove" />d.
	/// </summary>
	public sealed class MultiAnalyser
		: IAnalyserGroup
		, IDisposable
	{
		private readonly List<DataSourceAnalyser> _analyser;
		private readonly IAnalysisEngine _analysisEngine;
		private readonly IDataSource _dataSource;

		public MultiAnalyser(IDataSource dataSource, IAnalysisEngine analysisEngine)
		{
			_dataSource = dataSource;
			_analysisEngine = analysisEngine;
			_analyser = new List<DataSourceAnalyser>();
		}

		public void Dispose()
		{
			foreach (var analyser in _analyser)
				analyser.Dispose();
		}

		/// <summary>
		/// </summary>
		/// <param name="logAnalyserType"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public IDataSourceAnalyser Add(Type logAnalyserType, ILogAnalyserConfiguration configuration)
		{
			var analyser = new DataSourceAnalyser(_dataSource, _analysisEngine, logAnalyserType);
			try
			{
				analyser.Configuration = configuration;
				_analyser.Add(analyser);
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
			if (_analyser.Remove(tmp))
				tmp?.Dispose();
		}
	}
}