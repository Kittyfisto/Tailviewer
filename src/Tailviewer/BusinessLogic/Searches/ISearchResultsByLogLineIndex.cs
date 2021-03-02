using System.Collections.Generic;
using Tailviewer.Api;

namespace Tailviewer.BusinessLogic.Searches
{
	public interface ISearchResultsByLogLineIndex
		: IReadOnlyDictionary<LogLineIndex, IEnumerable<LogLineMatch>>
	{}
}