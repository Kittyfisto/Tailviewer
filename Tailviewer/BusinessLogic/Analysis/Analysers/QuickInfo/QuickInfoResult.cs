using System.Linq;

namespace Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo
{
	public sealed class QuickInfoResult
		: ILogAnalysisResult
	{
		public string[] Values;

		public object Clone()
		{
			return new QuickInfoResult
			{
				Values = Values?.ToArray()
			};
		}
	}
}