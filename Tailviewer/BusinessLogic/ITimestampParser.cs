using System;

namespace Tailviewer.BusinessLogic
{
	public interface ITimestampParser
	{
		bool TryParse(string content, out DateTime timestamp);
	}
}