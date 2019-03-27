using System.Collections.Generic;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Searches
{
	public interface ISearchResultsByLogLineIndex
		: IReadOnlyDictionary<LogLineIndex, IEnumerable<LogLineMatch>>
	{}
}