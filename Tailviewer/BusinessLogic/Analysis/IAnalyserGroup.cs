using System;
using Tailviewer.BusinessLogic.Analysis.Analysers;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     Responsible for holding one or more <see cref="IDataSourceAnalyser" />:
	///     They can be <see cref="Add" />ed as well as <see cref="Remove" />d.
	/// </summary>
	public interface IAnalyserGroup
	{
		/// <summary>
		/// </summary>
		/// <param name="logAnalyserType"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		IDataSourceAnalyser Add(Type logAnalyserType, ILogAnalyserConfiguration configuration);

		/// <summary>
		/// </summary>
		/// <param name="analyser"></param>
		void Remove(IDataSourceAnalyser analyser);
	}
}