using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Searches
{
	public interface ILogFileSearchListener
	{
		void OnSearchModified(ILogFileSearch sender, List<LogMatch> matches);
	}
}