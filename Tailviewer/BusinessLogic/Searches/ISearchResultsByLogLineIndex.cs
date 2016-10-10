using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Searches
{
	public interface ISearchResultsByLogLineIndex
		: IReadOnlyDictionary<LogLineIndex, IEnumerable<LogLineMatch>>
	{}
}