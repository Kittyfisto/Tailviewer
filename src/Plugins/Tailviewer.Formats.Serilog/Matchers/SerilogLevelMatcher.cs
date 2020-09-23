using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Formats.Serilog.Matchers
{
	public sealed class SerilogLevelMatcher
		: ISerilogMatcher
	{
		private readonly int _groupIndex;
		private readonly IReadOnlyDictionary<string, LevelFlags> _mapping;
		private readonly string _regex;

		public SerilogLevelMatcher(string specifier, int groupIndex)
		{
			_groupIndex = groupIndex;
			_mapping = CreateMapping(specifier);
			_regex = string.Format("({0})", string.Join("|", _mapping.Keys));
		}

		[Pure]
		private static IReadOnlyDictionary<string, LevelFlags> CreateMapping(string specifier)
		{
			if (string.IsNullOrEmpty(specifier))
				return new Dictionary<string, LevelFlags>
				{
					{"Verbose", LevelFlags.Trace },
					{"Debug", LevelFlags.Debug },
					{"Information", LevelFlags.Info },
					{"Warning", LevelFlags.Warning },
					{"Error", LevelFlags.Error },
					{"Fatal", LevelFlags.Fatal }
				};

			switch (specifier)
			{
				case "u1":
					return new Dictionary<string, LevelFlags>
					{
						{"V", LevelFlags.Trace },
						{"D", LevelFlags.Debug },
						{"I", LevelFlags.Info },
						{"W", LevelFlags.Warning },
						{"E", LevelFlags.Error },
						{"F", LevelFlags.Fatal }
					};

				case "u2":
					return new Dictionary<string, LevelFlags>
					{
						{"VB", LevelFlags.Trace },
						{"DE", LevelFlags.Debug },
						{"IN", LevelFlags.Info },
						{"WN", LevelFlags.Warning },
						{"ER", LevelFlags.Error },
						{"FA", LevelFlags.Fatal }
					};

				case "u3":
					return new Dictionary<string, LevelFlags>
					{
						{"VRB", LevelFlags.Trace },
						{"DBG", LevelFlags.Debug },
						{"INF", LevelFlags.Info },
						{"WRN", LevelFlags.Warning },
						{"ERR", LevelFlags.Error },
						{"FTL", LevelFlags.Fatal }
					};

				case "u4":
					return new Dictionary<string, LevelFlags>
					{
						{"VERB", LevelFlags.Trace },
						{"DBUG", LevelFlags.Debug },
						{"INFO", LevelFlags.Info },
						{"WARN", LevelFlags.Warning },
						{"EROR", LevelFlags.Error },
						{"FATL", LevelFlags.Fatal }
					};

				case "u5":
					return new Dictionary<string, LevelFlags>
					{
						{"VERBO", LevelFlags.Trace },
						{"DEBUG", LevelFlags.Debug },
						{"INFOR", LevelFlags.Info },
						{"WARNI", LevelFlags.Warning },
						{"ERROR", LevelFlags.Error },
						{"FATAL", LevelFlags.Fatal }
					};

				case "u6":
					return new Dictionary<string, LevelFlags>
					{
						{"VERBOS", LevelFlags.Trace },
						{"DEBUG", LevelFlags.Debug },
						{"INFORM", LevelFlags.Info },
						{"WARNIN", LevelFlags.Warning },
						{"ERROR", LevelFlags.Error },
						{"FATAL", LevelFlags.Fatal }
					};

				case "u7":
					return new Dictionary<string, LevelFlags>
					{
						{"VERBOSE", LevelFlags.Trace },
						{"DEBUG", LevelFlags.Debug },
						{"INFORMA", LevelFlags.Info },
						{"WARNING", LevelFlags.Warning },
						{"ERROR", LevelFlags.Error },
						{"FATAL", LevelFlags.Fatal }
					};

				case "u8":
					return new Dictionary<string, LevelFlags>
					{
						{"VERBOSE", LevelFlags.Trace },
						{"DEBUG", LevelFlags.Debug },
						{"INFORMAT", LevelFlags.Info },
						{"WARNING", LevelFlags.Warning },
						{"ERROR", LevelFlags.Error },
						{"FATAL", LevelFlags.Fatal }
					};

				case "u9":
					return new Dictionary<string, LevelFlags>
					{
						{"VERBOSE", LevelFlags.Trace },
						{"DEBUG", LevelFlags.Debug },
						{"INFORMATI", LevelFlags.Info },
						{"WARNING", LevelFlags.Warning },
						{"ERROR", LevelFlags.Error },
						{"FATAL", LevelFlags.Fatal }
					};

				case "u10":
					return new Dictionary<string, LevelFlags>
					{
						{"VERBOSE", LevelFlags.Trace },
						{"DEBUG", LevelFlags.Debug },
						{"INFORMATIO", LevelFlags.Info },
						{"WARNING", LevelFlags.Warning },
						{"ERROR", LevelFlags.Error },
						{"FATAL", LevelFlags.Fatal }
					};

				case "u11":
					return new Dictionary<string, LevelFlags>
					{
						{"VERBOSE", LevelFlags.Trace },
						{"DEBUG", LevelFlags.Debug },
						{"INFORMATION", LevelFlags.Info },
						{"WARNING", LevelFlags.Warning },
						{"ERROR", LevelFlags.Error },
						{"FATAL", LevelFlags.Fatal }
					};

				default:
					throw new ArgumentException($"Unknown log level specifier: {specifier}");
			}
		}

		#region Implementation of ISerilogMatcher

		public string Regex
		{
			get { return _regex; }
		}

		public int NumGroups
		{
			get { return 1; }
		}

		public ILogFileColumn Column
		{
			get { return LogFileColumns.LogLevel; }
		}

		public void MatchInto(Match match, SerilogEntry logEntry)
		{
			var capture = match.Groups[_groupIndex].Value;
			logEntry.LogLevel = _mapping[capture];
		}

		#endregion
	}
}