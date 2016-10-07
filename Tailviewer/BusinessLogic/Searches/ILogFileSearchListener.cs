using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Searches
{
	public interface ILogFileSearchListener
	{
		void OnSearchModified(List<LogMatch> matches);
	}
}