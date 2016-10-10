using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Searches
{
	public interface ILogFileSearchListener
	{
		void OnSearchModified(ILogFileSearch sender, IEnumerable<LogMatch> matches);
	}
}