using System;
using System.Collections.Generic;

namespace Tailviewer.Core.Filters.ExpressionEngine
{
	internal sealed class LogLevelLiteral
		: IExpression<LevelFlags>
	{
		private static readonly IReadOnlyDictionary<string, LevelFlags> LogLevels;

		private readonly LevelFlags _logLevel;

		static LogLevelLiteral()
		{
			LogLevels = new Dictionary<string, LevelFlags>
			{
				{"fatal", LevelFlags.Fatal},
				{"error", LevelFlags.Error},
				{"warning", LevelFlags.Warning},
				{"info", LevelFlags.Info},
				{"debug", LevelFlags.Debug},
				{"trace", LevelFlags.Trace},
				{"other", LevelFlags.Other}
			};
		}

		public LogLevelLiteral(LevelFlags logLevel)
		{
			_logLevel = logLevel;
		}

		#region Implementation of IExpression

		public Type ResultType => typeof(LevelFlags);

		public LevelFlags Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return _logLevel;
		}

		object IExpression.Evaluate(IReadOnlyList<IReadOnlyLogEntry> logEntry)
		{
			return Evaluate(logEntry);
		}

		#endregion

		#region Equality members

		private bool Equals(LogLevelLiteral other)
		{
			return _logLevel == other._logLevel;
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is LogLevelLiteral other && Equals(other);
		}

		public override int GetHashCode()
		{
			return (int) _logLevel;
		}

		public static bool operator ==(LogLevelLiteral left, LogLevelLiteral right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(LogLevelLiteral left, LogLevelLiteral right)
		{
			return !Equals(left, right);
		}

		#endregion

		#region Overrides of Object

		public override string ToString()
		{
			return _logLevel.ToString();
		}

		#endregion

		public static bool TryParse(string value, out LevelFlags logLevel)
		{
			foreach (var pair in LogLevels)
			{
				if (string.Equals(pair.Key, value, StringComparison.InvariantCultureIgnoreCase))
				{
					logLevel = pair.Value;
					return true;
				}
			}

			logLevel = LevelFlags.None;
			return false;
		}
	}
}
