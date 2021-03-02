using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     Responsible for finding the first matching timestamp format in a stream of messages.
	///     Once a format has been found, all subsequent messages are only evaluated agains the given format.
	/// </summary>
	public sealed class TimestampParser
		: ITimestampParser
	{
		private const int MaxToleratedExceptions = 10;

		/// <summary>
		/// The maximum number of characters (from the start of the line) which tailviewer will traverse in order to find a timestamp.
		/// </summary>
		/// <remarks>
		/// This number exists because sometimes a software (not to name names) might write utter garbage into its first line.
		/// If there's enough garbage (say 158k bytes) in the first line, then tailviewer would search endlessly for a timestamp
		/// (the loop not optimized so searching that much data might take hours).
		/// If one were to find a better way to implement the loop, then this number might become irrelevant once more.
		/// </remarks>
		private const int MaxLineLength = 200;

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ITimestampParser[] _subParsers;
		private readonly int _minimumLength;

		/// <summary>
		/// The parser (if any) which has been successful in the past and will be used from now on.
		/// </summary>
		private ITimestampParser _determinedParser;
		private int _numExceptions;

		/// <summary>
		///     Initializes this parser.
		/// </summary>
		public TimestampParser()
			: this(
				// The format I currently use at work - should be supported by default :P
				new DateTimeParser("yyyy-MM-dd HH:mm:ss,fff"),

				// Request by abani1986 (https://github.com/Kittyfisto/Tailviewer/issues/182)
				new DateTimeParser("yyyy-MM-dd HH:mm:ss:fff"),

				new DateTimeParser("yyyy-MM-dd HH:mm:ss.fff"),

				new DateTimeParser("yyyy-MM-dd HH:mm:ss"),

				// Request by abani1986 (https://github.com/Kittyfisto/Tailviewer/issues/182)
				new DateTimeParser("dd/MM/yyyy HH:mm:ss:fff"),

				// Another one used by a colleague, well its actually nanoseconds but I can't find that format string
				new DateTimeParser("yyyy MMM dd HH:mm:ss.fff"),
				new DateTimeParser("yyyy MMM dd HH:mm:ss"),
				new DateTimeParser("yyyy-MM-dd HH-mm-ss.fff"),
				new DateTimeParser("yyyy-MM-dd HH-mm-ss"),

				// Various formats...
				new DateTimeParser("dd/MMM/yyyy:HH:mm:ss"),
				new DateTimeParser("ddd MMM dd HH:mm:ss.fff yyyy"),

				// One of the most bizare formats: Time of day is apparently not interesting enough, just as are fractions of a second.
				// We do, however, get the seconds (in nano seconds) since the start of the application...
				new TimeOfDaySecondsSinceStartParser(),
				new DateTimeParser("HH:mm:ss.fff"),
				new DateTimeParser("HH:mm:ss")
			)
		{
		}

		/// <summary>
		///     Initializes this parser with the given parsers.
		/// </summary>
		public TimestampParser(params ITimestampParser[] parsers)
		{
			if (parsers == null)
				throw new ArgumentNullException(nameof(parsers));

			_subParsers = parsers;
			if (parsers.Length > 0)
			{
				_minimumLength = int.MaxValue;
				foreach (var parser in parsers)
				{
					_minimumLength = Math.Min(_minimumLength, parser.MinimumLength);
				}
			}
			else
			{
				_minimumLength = 0;
			}
		}

		/// <summary>
		///     The index of the character where the timestamp is expected to start.
		/// </summary>
		public int? DateTimeColumn { get; private set; }

		/// <summary>
		///     The length of the expected timestamp.
		/// </summary>
		public int? DateTimeLength { get; private set; }

		/// <inheritdoc />
		public int MinimumLength => _minimumLength;

		/// <inheritdoc />
		public bool TryParse(string content, out DateTime timestamp)
		{
			if (_numExceptions > MaxToleratedExceptions)
			{
				timestamp = DateTime.MinValue;
				return false;
			}

			try
			{
				if (DateTimeColumn == null || DateTimeLength == null)
					DetermineDateTimePart(content);

				return TryParseTimestamp(content, out timestamp);
			}
			catch (Exception e)
			{
				++_numExceptions;
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				timestamp = DateTime.MinValue;
				return false;
			}
		}

		private bool TryParseTimestamp(string content, out DateTime timestamp)
		{
			if (_determinedParser != null && DateTimeColumn != null && DateTimeLength != null)
			{
				var start = DateTimeColumn.Value;
				var length = DateTimeLength.Value;
				if (content.Length >= start + length)
				{
					var timestampValue = content.Substring(start, length);
					if (_determinedParser.TryParse(timestampValue, out timestamp))
						return true;
				}
			}

			timestamp = DateTime.MinValue;
			return false;
		}

		private void DetermineDateTimePart(string line)
		{
			if (SkipLine(line))
				return;

			int lineLength = line.Length;
			if (line.Length > MaxLineLength)
			{
				Log.WarnFormat("Line has a length ({1}) greater than {0} characters. A timestamp will only be looked for in the first {0} characters!", MaxLineLength, line.Length);
				lineLength = MaxLineLength;
			}

			foreach (var parser in _subParsers)
			{
				for (var firstIndex = 0; firstIndex < lineLength; ++firstIndex)
				for (var lastIndex = firstIndex + parser.MinimumLength; lastIndex <= lineLength; ++lastIndex)
				{
					var dateTimeString = line.Substring(firstIndex, lastIndex - firstIndex);
					try
					{
						DateTime unused;
						if (parser.TryParse(dateTimeString, out unused))
						{
							var length = lastIndex - firstIndex;
							DateTimeColumn = firstIndex;
							DateTimeLength = length;
							_determinedParser = parser;
							return;
						}
					}
					catch (Exception e)
					{
						++_numExceptions;
						Log.ErrorFormat("Caught unexpected exception: {0}", e);
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		///   This method exists because parsing timestamps is time consuming due to its sub-par implementation.
		/// </remarks>
		/// <param name="line"></param>
		/// <returns></returns>
		[Pure]
		private static bool SkipLine(string line)
		{
			// The runtime of the DetermineDateTimePart() method is so bad that performing a simple o(n)
			// check is totally worth it, when you can skip gigantic lines
			if (line.Any(char.IsDigit))
				return false;

			return true;
		}
	}
}