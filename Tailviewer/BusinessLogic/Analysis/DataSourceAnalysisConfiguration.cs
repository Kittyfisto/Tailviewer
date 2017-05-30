using System;
using Tailviewer.BusinessLogic.Analysis.Analysers;

namespace Tailviewer.BusinessLogic.Analysis
{
	public sealed class DataSourceAnalysisConfiguration
	{
		public Type LogAnalyserType { get; set; }

		/// <summary>
		/// The configuration forwarded to the <see cref="ILogAnalyser"/>.
		/// </summary>
		public ILogAnalyserConfiguration Configuration { get; set; }
	}
}