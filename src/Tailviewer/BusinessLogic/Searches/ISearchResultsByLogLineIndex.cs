using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Searches
{
	public interface ISearchResultsByLogLineIndex
		: IReadOnlyDictionary<LogLineIndex, IEnumerable<LogLineMatch>>
	{}
}