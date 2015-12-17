using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic
{
	internal interface IFilter
	{
		[Pure]
		bool PassesFilter(LogLine logLine);
	}
}