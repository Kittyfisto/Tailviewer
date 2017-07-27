using System;
using System.Reflection;
using log4net;

namespace Tailviewer.BusinessLogic.Parsers
{
	/// <summary>
	///     Responsible for finding the first matching timestamp format in a stream of messages.
	///     Once a format has been found, all subsequent messages are only evaluated agains the given format.
	/// </summary>
	public sealed class TimestampParser
		: ITimestampParser
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private const int MaxToleratedExceptions = 10;

		private readonly ITimestampParser[] _subParsers;
		private int? _dateTimeColumn;
		private int? _dateTimeFormatIndex;
		private int? _dateTimeLength;
		private int _numExceptions;

		public int? DateTimeColumn => _dateTimeColumn;

		public int? DateTimeLength => _dateTimeLength;

		public TimestampParser()
			: this(
				// The format I currently use at work - should be supported by default :P
				new DateTimeParser("yyyy-MM-dd HH:mm:ss,fff"),
				new DateTimeParser("yyyy-MM-dd HH:mm:ss"),

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
				new DateTimeParser("HH:mm:ss")
			)
		{}

		public TimestampParser( params ITimestampParser[] parsers)
		{
			if (parsers == null)
				throw new ArgumentNullException(nameof(parsers));

			_subParsers = parsers;
		}

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
				if (_dateTimeColumn == null || _dateTimeLength == null)
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
			if (_dateTimeFormatIndex != null && _dateTimeColumn != null && _dateTimeLength != null)
			{
				var start = _dateTimeColumn.Value;
				var length = _dateTimeLength.Value;
				if (content.Length >= start + length)
				{
					var timestampValue = content.Substring(start, length);
					var parser = _subParsers[_dateTimeFormatIndex.Value];
					if (parser.TryParse(timestampValue, out timestamp))
						return true;
				}
			}

			timestamp = DateTime.MinValue;
			return false;
		}

		private void DetermineDateTimePart(string line)
		{
			for (var m = 0; m < _subParsers.Length; ++m)
			for (var i = 0; i < line.Length; ++i)
			for (var n = i; n <= line.Length; ++n)
			{
				var dateTimeString = line.Substring(i, n - i);
				try
				{
					DateTime unused;
					if (_subParsers[m].TryParse(dateTimeString, out unused))
					{
						var length = n - i;
						_dateTimeColumn = i;
						_dateTimeLength = length;
						_dateTimeFormatIndex = m;
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
}