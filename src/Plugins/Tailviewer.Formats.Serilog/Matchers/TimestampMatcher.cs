using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.Formats.Serilog.Matchers
{
	public sealed class TimestampMatcher
		: ISerilogMatcher
	{
		private static readonly IReadOnlyList<KeyValuePair<string, Func<IDateTimeMatcher>>> MatcherFactories;

		private readonly int _groupIndex;
		private readonly IReadOnlyList<IDateTimeMatcher> _matchers;
		private readonly string _regex;
		private readonly int _numGroups;

		static TimestampMatcher()
		{
			MatcherFactories = new List<KeyValuePair<string, Func<IDateTimeMatcher>>>
			{
				new KeyValuePair<string, Func<IDateTimeMatcher>>("fffffff", () => new fffffffMatcher() ),
				new KeyValuePair<string, Func<IDateTimeMatcher>>("ffffff", () => new ffffffMatcher() ),
				new KeyValuePair<string, Func<IDateTimeMatcher>>("fffff", () => new fffffMatcher() ),
				new KeyValuePair<string, Func<IDateTimeMatcher>>("ffff", () => new ffffMatcher() ),
				new KeyValuePair<string, Func<IDateTimeMatcher>>("fff", () => new fffMatcher() ),
				new KeyValuePair<string, Func<IDateTimeMatcher>>("ff", () => new ffMatcher() ),
				new KeyValuePair<string, Func<IDateTimeMatcher>>("f", () => new fMatcher() ),

				new KeyValuePair<string, Func<IDateTimeMatcher>>("ss", () => new ssMatcher() ),
				new KeyValuePair<string, Func<IDateTimeMatcher>>("s", () => new sMatcher() ),

				new KeyValuePair<string, Func<IDateTimeMatcher>>("mm", () => new mmMatcher() ),
				new KeyValuePair<string, Func<IDateTimeMatcher>>("m", () => new mMatcher() ),

				new KeyValuePair<string, Func<IDateTimeMatcher>>("HH", () => new HHMatcher() ),
				new KeyValuePair<string, Func<IDateTimeMatcher>>("H", () => new HMatcher() ),

				new KeyValuePair<string, Func<IDateTimeMatcher>>("dd", () => new ddMatcher() ),
				new KeyValuePair<string, Func<IDateTimeMatcher>>("d", () => new dMatcher() ),

				new KeyValuePair<string, Func<IDateTimeMatcher>>("MM", () => new MMMatcher() ),
				new KeyValuePair<string, Func<IDateTimeMatcher>>("M", () => new MMatcher() ),

				new KeyValuePair<string, Func<IDateTimeMatcher>>("yyyy", () => new yyyyMatcher()),

				new KeyValuePair<string, Func<IDateTimeMatcher>>("K", () => new KMatcher())
			};
		}

		public TimestampMatcher(string specifier, int groupIndex)
		{
			var regex = new StringBuilder();
			var matchers = new List<IDateTimeMatcher>();
			for (int i = 0; i < specifier.Length;)
			{
				var matcher = TryCreateMatcher(specifier, ref i);
				if (matcher != null)
				{
					regex.Append(matcher.Regex);
					matchers.Add(matcher);
					_numGroups += matcher.NumGroups;
				}
				else
				{
					regex.Append(Regex.Escape(specifier.Substring(i, 1)));
					i += 1;
				}
			}

			_groupIndex = groupIndex;
			_matchers = matchers;
			_regex = regex.ToString();
		}

		#region Implementation of ISerilogMatcher

		string ISerilogMatcher.Regex
		{
			get { return _regex; }
		}

		public int NumGroups
		{
			get { return _numGroups; }
		}

		public IColumnDescriptor Column
		{
			get { return Columns.Timestamp; }
		}

		public void MatchInto(Match match, LogEntry logEntry)
		{
			var tmpValue = new TmpDateTime();
			int groupIndex = _groupIndex;
			for(int i = 0; i < _matchers.Count; ++i)
			{
				_matchers[i].MatchInto(match, ref tmpValue, groupIndex);
				groupIndex += _matchers[i].NumGroups;
			}

			var dateTime = tmpValue.ToDateTime();
			logEntry.Timestamp = dateTime;
		}

		#endregion

		[Pure]
		private static bool ContainsAt(string value, string searchTerm, int startIndex)
		{
			if (value.Length - startIndex < searchTerm.Length)
				return false;

			for (int i = 0; i < searchTerm.Length; ++i)
			{
				if (value[startIndex + i] != searchTerm[i])
					return false;
			}

			return true;
		}

		private static IDateTimeMatcher TryCreateMatcher(string specifier,
		                                                 ref int startIndex)
		{
			foreach (var pair in MatcherFactories)
			{
				if (ContainsAt(specifier, pair.Key, startIndex))
				{
					var matcher = pair.Value();
					startIndex += pair.Key.Length;
					return matcher;
				}
			}

			var spec = specifier[startIndex];
			if (char.IsLetter(spec))
				throw new ArgumentException($"Unsupported Timestamp specifier '{spec}' in format '{specifier}'");

			return null;
		}
		struct TmpDateTime
		{
			public int Year;
			public int Month;
			public int Day;
			public int Hour;
			public int Minute;
			public int Second;
			public int Ticks;

			public DateTime ToDateTime()
			{
				return new DateTime(Year, Month, Day, Hour, Minute, Second)
					+ TimeSpan.FromTicks(Ticks);
			}
		}

		interface IDateTimeMatcher
		{
			string Regex { get; }
			int NumGroups { get; }
			void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex);
		}

		// ReSharper disable once InconsistentNaming
		sealed class fMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d)"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Ticks = int.Parse(value) * 1000000;
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class ffMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{2})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Ticks = int.Parse(value) * 100000;
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class fffMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{3})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Ticks = int.Parse(value) * 10000;
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class ffffMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{4})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Ticks = int.Parse(value) * 1000;
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class fffffMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{5})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Ticks = int.Parse(value) * 100;
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class ffffffMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{6})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Ticks = int.Parse(value) * 10;
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class fffffffMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{67})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Ticks = int.Parse(value);
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class sMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{1,2})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Second = int.Parse(value, NumberStyles.Integer, CultureInfo.CurrentUICulture);
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class ssMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{2})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Second = int.Parse(value, NumberStyles.Integer, CultureInfo.CurrentUICulture);
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class mMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{1,2})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Minute = int.Parse(value, NumberStyles.Integer, CultureInfo.CurrentUICulture);
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class mmMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{2})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Minute = int.Parse(value, NumberStyles.Integer, CultureInfo.CurrentUICulture);
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class dMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{1,2})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Day = int.Parse(value, NumberStyles.Integer, CultureInfo.CurrentUICulture);
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class ddMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Day = int.Parse(value, NumberStyles.Integer, CultureInfo.CurrentUICulture);
			}

			public string Regex
			{
				get { return @"(\d{2})"; }
			}

			#endregion
		}

		sealed class HMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{1,2})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Hour = int.Parse(value);
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class HHMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{2})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Hour = int.Parse(value);
			}

			#endregion
		}

		sealed class MMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{1,2})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Month = int.Parse(value);
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class MMMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{2})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Month = int.Parse(value);
			}

			#endregion
		}

		// ReSharper disable once InconsistentNaming
		sealed class yyyyMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"(\d{4})"; }
			}

			public int NumGroups
			{
				get { return 1; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				var value = match.Groups[groupIndex].Value;
				dateTime.Year = int.Parse(value);
			}

			#endregion
		}

		sealed class KMatcher
			: IDateTimeMatcher
		{
			#region Implementation of IDateTimeMatcher

			public string Regex
			{
				get { return @"((\+|-)\d{2}:\d{2})"; }
			}

			public int NumGroups
			{
				get { return 2; }
			}

			public void MatchInto(Match match, ref TmpDateTime dateTime, int groupIndex)
			{
				// For now we'll ignore this value
			}

			#endregion
		}
	}
}
