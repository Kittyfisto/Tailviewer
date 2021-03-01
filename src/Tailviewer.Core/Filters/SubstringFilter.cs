using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Tailviewer.Core.Filters
{
	/// <summary>
	///     A filter that looks for a substring in a (possibly) bigger string.
	/// </summary>
	internal sealed class SubstringFilter
		: ILogEntryFilter
	{
		private readonly StringComparison _comparison;
		private readonly string _stringFilter;

		/// <summary>
		///     Initializes this filter.
		/// </summary>
		/// <param name="stringFilter"></param>
		/// <param name="ignoreCase"></param>
		public SubstringFilter(string stringFilter, bool ignoreCase)
		{
			if (string.IsNullOrEmpty(stringFilter))
				throw new ArgumentException("stringFilter may not be empty");

			_stringFilter = stringFilter;
			_comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
		}

		/// <inheritdoc />
		public bool PassesFilter(IEnumerable<IReadOnlyLogEntry> logEntry)
		{
			// ReSharper disable LoopCanBeConvertedToQuery
			foreach (IReadOnlyLogEntry logLine in logEntry)
				// ReSharper restore LoopCanBeConvertedToQuery
			{
				if (PassesFilter(logLine))
					return true;
			}

			return false;
		}

		/// <inheritdoc />
		[Pure]
		public bool PassesFilter(IReadOnlyLogEntry logLine)
		{
			var rawContent = logLine.RawContent;
			if (rawContent == null)
				return false;

			int idx = rawContent.IndexOf(_stringFilter, _comparison);
			if (idx == -1)
				return false;

			return true;
		}

		/// <inheritdoc />
		public List<LogLineMatch> Match(IReadOnlyLogEntry line)
		{
			var ret = new List<LogLineMatch>();
			Match(line, ret);
			return ret;
		}

		/// <inheritdoc />
		public void Match(IReadOnlyLogEntry line, List<LogLineMatch> matches)
		{
			var rawContent = line.RawContent;
			if (rawContent == null)
				return;

			int startIndex = 0;
			do
			{
				startIndex = rawContent.IndexOf(_stringFilter, startIndex, _comparison);
				if (startIndex < 0)
					break;

				var length = _stringFilter.Length;
				matches.Add(new LogLineMatch(startIndex, length));
				startIndex += length;
			} while (startIndex < rawContent.Length - 1);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("message.Contains({0}, {1})", _stringFilter, _comparison);
		}
	}
}