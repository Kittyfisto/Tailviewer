using System.Collections.Generic;
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

		private FilterExpression(IExpression expression)
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
		public bool PassesFilter(LogLine logLine)
		{
			return PassesFilter(new[] {logLine});
		}
		
		/// <inheritdoc />
		public List<LogLineMatch> Match(LogLine line)
		{
			return new List<LogLineMatch>();
		}

		#endregion

		#region Implementation of ILogEntryFilter
		
		/// <inheritdoc />
		public bool PassesFilter(IEnumerable<LogLine> logEntry)
		{
			var result = _expression.Evaluate(logEntry);
			return Equals(result, true);
		}
		
		/// <inheritdoc />
		public void Match(LogLine line, List<LogLineMatch> matches)
		{}

		#endregion
	}
}
