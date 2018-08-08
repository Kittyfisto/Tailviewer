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

		public abstract object Evaluate(IEnumerable<LogLine> logEntry);

		#endregion

		[Pure]
		public static IExpression Create(string value)
		{
			if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integerValue))
			{
				return new IntegerLiteral(integerValue);
			}

			throw new NotImplementedException();
		}
	}
}
