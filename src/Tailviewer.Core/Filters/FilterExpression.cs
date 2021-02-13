using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Filters.ExpressionEngine;

namespace Tailviewer.Core.Filters
{
	/// <summary>
	/// Parses a user supplied string into a boolean expression.
	/// </summary>
	public sealed class FilterExpression
		: ILogEntryFilter
	{
		private readonly IExpression _expression;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="expression"></param>
		public FilterExpression(IExpression expression)
		{
			_expression = expression;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		public static FilterExpression Parse(string expression)
		{
			var parser = new ExpressionParser();
			var expr = parser.Parse(expression);
			return new FilterExpression(expr);
		}

		#region Implementation of ILogLineFilter

		/// <inheritdoc />
		public bool PassesFilter(IReadOnlyLogEntry logLine)
		{
			return PassesFilter(new[] {logLine});
		}
		
		/// <inheritdoc />
		public List<LogLineMatch> Match(IReadOnlyLogEntry line)
		{
			return new List<LogLineMatch>();
		}

		#endregion

		#region Implementation of ILogEntryFilter
		
		/// <inheritdoc />
		public bool PassesFilter(IEnumerable<IReadOnlyLogEntry> logEntry)
		{
			var result = _expression.Evaluate(logEntry.ToList());
			return Equals(result, true);
		}
		
		/// <inheritdoc />
		public void Match(IReadOnlyLogEntry line, List<LogLineMatch> matches)
		{}

		#endregion
	}
}
