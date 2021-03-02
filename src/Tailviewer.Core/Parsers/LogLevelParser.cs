using System;
using System.Diagnostics.Contracts;
using Tailviewer.Api;

namespace Tailviewer.Core.Parsers
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class LogLevelParser
	{
		/// <summary>
		///     Parses the given line and returns the left most log level
		///     from it.
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		[Pure]
		public LevelFlags DetermineLevelFromLine(string line)
		{
			DetermineLevelsFromLine(line, out var leftMost);
			return leftMost;
		}

		/// <summary>
		///     Parses the given line and extracts log4net levels from it,
		///     if there are any.
		/// </summary>
		/// <param name="line"></param>
		/// <param name="leftMost">The left-most log level in the given <paramref name="line"/></param>
		/// <returns>The last log level in the given <paramref name="line"/></returns>
		public LevelFlags DetermineLevelsFromLine(string line, out LevelFlags leftMost)
		{
			LevelFlags rightMost = LevelFlags.None;
			leftMost = LevelFlags.None;
			int index = int.MaxValue;

			if (line == null)
			{
				leftMost = LevelFlags.Other;
				return LevelFlags.Other;
			}

			var comparison = StringComparison.InvariantCulture;
			var idx = line.IndexOf("FATAL", comparison);
			if (idx != -1)
			{
				if (idx < index)
				{
					leftMost = LevelFlags.Fatal;
					index = idx;
				}
			}

			idx = line.IndexOf("ERROR", comparison);
			if (idx != -1)
			{
				rightMost |= LevelFlags.Error;
				if (idx < index)
				{
					leftMost = LevelFlags.Error;
					index = idx;
				}
			}

			idx = line.IndexOf("WARN", comparison);
			if (idx != -1)
			{
				rightMost |= LevelFlags.Warning;
				if (idx < index)
				{
					leftMost = LevelFlags.Warning;
					index = idx;
				}
			}

			idx = line.IndexOf("INFO", comparison);
			if (idx != -1)
			{
				rightMost |= LevelFlags.Info;
				if (idx < index)
				{
					leftMost = LevelFlags.Info;
					index = idx;
				}
			}

			idx = line.IndexOf("DEBUG", comparison);
			if (idx != -1)
			{
				rightMost |= LevelFlags.Debug;
				if (idx < index)
				{
					leftMost = LevelFlags.Debug;
					index = idx;
				}
			}

			idx = line.IndexOf("TRACE", comparison);
			if (idx != -1)
			{
				rightMost |= LevelFlags.Trace;
				if (idx < index)
				{
					leftMost = LevelFlags.Trace;
					index = idx;
				}
			}

			if (leftMost == LevelFlags.None)
				leftMost = LevelFlags.Other;

			if (rightMost == LevelFlags.None)
				rightMost = LevelFlags.Other;
			return rightMost;
		}
	}
}
