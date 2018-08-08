using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	/// <summary>
	/// 
	/// </summary>
	public interface IExpression
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		object Evaluate(IEnumerable<LogLine> logEntry);
	}
}
