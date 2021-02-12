using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal abstract class Literal
		: IExpression
	{
		#region Implementation of IExpression

		public abstract Type ResultType { get; }

		public abstract object Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry);

		#endregion

		[Pure]
		public static IExpression Create(string value)
		{
			if (DateTimeIntervalLiteral.TryParse(value, out var dateTimeLiteral))
				return new DateTimeIntervalLiteral(dateTimeLiteral);

			if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integerValue))
				return new IntegerLiteral(integerValue);

			throw new NotImplementedException();
		}
	}
}
