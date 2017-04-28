using System.Collections.Generic;
using Tailviewer.BusinessLogic.Dashboard.Analysers;

namespace Tailviewer.BusinessLogic.Dashboard
{
	public sealed class Dashboard
	{
		private readonly List<ILogAnalyser> _analysers;

		public Dashboard()
		{
			_analysers = new List<ILogAnalyser>();
		}
	}
}