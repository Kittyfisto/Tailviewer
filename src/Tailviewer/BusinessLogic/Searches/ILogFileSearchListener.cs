using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.Searches
{
	public interface ILogFileSearchListener
	{
		void OnSearchModified(ILogSourceSearch sender, IEnumerable<LogMatch> matches);
	}
}