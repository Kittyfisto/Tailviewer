using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class ContainsStringExpression
		: IExpression
	{
		private readonly IExpression<string> _lhs;
		private readonly IExpression<string> _rhs;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		public ContainsStringExpression(IExpression<string> lhs, IExpression<string> rhs)
		{
			_lhs = lhs;
			_rhs = rhs;
		}

		public static IExpression Create(IExpression<string> lhs, IExpression<string> rhs)
		{
			return new ContainsStringExpression(lhs, rhs);
		}

		public Type ResultType => typeof(bool);

		/// <inheritdoc />
		public object Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			var lhs = _lhs.Evaluate(logEntry);
			var rhs = _rhs.Evaluate(logEntry);
			if (lhs == null)
				return false;
			if (rhs == null)
				return true;
			return lhs.Contains(rhs);
		}

		#region Overrides of Object

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", _lhs, Tokenizer.ToString(TokenType.Contains), _rhs);
		}

		#region Equality members

		private bool Equals(ContainsStringExpression other)
		{
			return _lhs.Equals(other._lhs) && _rhs.Equals(other._rhs);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ContainsStringExpression && Equals((ContainsStringExpression) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_lhs.GetHashCode() * 397) ^ _rhs.GetHashCode();
			}
		}

		#endregion

		#endregion
	}
}