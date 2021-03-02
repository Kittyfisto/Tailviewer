using System;
using System.Collections.Generic;
using Metrolib;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     Maintains a collection of well-known log file properties which are applicable to most sources.
	/// </summary>
	public static class GeneralProperties
	{
		/// <summary>
		///     The number of log entries in the log source.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<int> LogEntryCount;

		#region Log Levels

		/// <summary>
		///     The number of log entries with a log level of <see cref="LevelFlags.Trace"/>.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<int> TraceLogEntryCount;
		
		/// <summary>
		///     The number of log entries with a log level of <see cref="LevelFlags.Debug"/>.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<int> DebugLogEntryCount;

		/// <summary>
		///     The number of log entries with a log level of <see cref="LevelFlags.Info"/>.
		/// </summary>

		public static readonly IReadOnlyPropertyDescriptor<int> InfoLogEntryCount;

		/// <summary>
		///     The number of log entries with a log level of <see cref="LevelFlags.Warning"/>.
		/// </summary>

		public static readonly IReadOnlyPropertyDescriptor<int> WarningLogEntryCount;

		/// <summary>
		///     The number of log entries with a log level of <see cref="LevelFlags.Error"/>.
		/// </summary>

		public static readonly IReadOnlyPropertyDescriptor<int> ErrorLogEntryCount;

		/// <summary>
		///     The number of log entries with a log level of <see cref="LevelFlags.Fatal"/>.
		/// </summary>

		public static readonly IReadOnlyPropertyDescriptor<int> FatalLogEntryCount;

		/// <summary>
		///     The number of log entries with a log level of <see cref="LevelFlags.Other"/>.
		/// </summary>

		public static readonly IReadOnlyPropertyDescriptor<int> OtherLogEntryCount;

		#endregion

		/// <summary>
		///     The name of the log file (for example the file name of a text log file).
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<string> Name;

		/// <summary>
		///     The first identified timestamp of the data source, if any, null otherwise.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<DateTime?> StartTimestamp;

		/// <summary>
		///     The last identified timestamp of the data source, if any, null otherwise.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<DateTime?> EndTimestamp;

		/// <summary>
		///     The difference between the first and last timestamp.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<TimeSpan?> Duration;

		/// <summary>
		///     The timestamp (in local time) the data source has last been modified.
		///     A modification is meant to be the addition and/or removal of at least one log line.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<DateTime?> LastModified;

		/// <summary>
		///     The timestamp (in local time) the data source has been created.
		/// </summary>
		/// <remarks>
		///     Is set to null when the data source doesn't exist.
		/// </remarks>
		public static readonly IReadOnlyPropertyDescriptor<DateTime?> Created;

		/// <summary>
		///     The approximate size of the data source.
		///     Is only needed to be displayed to the user.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<Size?> Size;

		/// <summary>
		///     The percentage of the file which has been fully processed already.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<Percentage> PercentageProcessed;

		/// <summary>
		///     The error, if any, which describes why this log file is empty.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<ErrorFlags> EmptyReason;

		/// <summary>
		///     The format of the log file, as determined by a <see cref="ILogFileFormatMatcher"/>.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<ILogFileFormat> Format;

		/// <summary>
		///     The certainty with which <see cref="ILogFileFormatMatcher"/> has detected the format.
		///     It is possible that while tailing a file, the format may change in case another format
		///     is now a better match.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<Certainty> FormatDetectionCertainty;

		/// <summary>
		///     The minimum set of properties a log file is expected to provide.
		/// </summary>
		public static readonly IReadOnlyList<IReadOnlyPropertyDescriptor> Minimum;

		static GeneralProperties()
		{
			var category = "general";

			LogEntryCount = new WellKnownReadOnlyProperty<int>(new[] {category, "log_entry_count"});

			TraceLogEntryCount = new WellKnownReadOnlyProperty<int>(new[] {category, "trace_log_entry_count"});
			DebugLogEntryCount = new WellKnownReadOnlyProperty<int>(new[] {category, "debug_log_entry_count"});
			InfoLogEntryCount = new WellKnownReadOnlyProperty<int>(new[] {category, "info_log_entry_count"});
			WarningLogEntryCount = new WellKnownReadOnlyProperty<int>(new[] {category, "warning_log_entry_count"});
			ErrorLogEntryCount = new WellKnownReadOnlyProperty<int>(new[] {category, "error_log_entry_count"});
			FatalLogEntryCount = new WellKnownReadOnlyProperty<int>(new[] {category, "fatal_log_entry_count"});
			OtherLogEntryCount = new WellKnownReadOnlyProperty<int>(new[] {category, "other_log_entry_count"});

			Name = new WellKnownReadOnlyProperty<string>(new []{category, "name"});
			StartTimestamp = new WellKnownReadOnlyProperty<DateTime?>(new []{category, "start_timestamp"});
			EndTimestamp = new WellKnownReadOnlyProperty<DateTime?>(new []{category, "end_timestamp"});
			Duration = new WellKnownReadOnlyProperty<TimeSpan?>(new []{category, "duration"});
			LastModified = new WellKnownReadOnlyProperty<DateTime?>(new []{category, "last_modified"});
			Created = new WellKnownReadOnlyProperty<DateTime?>(new []{category, "created"});
			Size = new WellKnownReadOnlyProperty<Size?>(new []{category, "size"});

			PercentageProcessed = new WellKnownReadOnlyProperty<Percentage>(new []{category, "percentage_processed"}, Percentage.Zero);
			EmptyReason = new WellKnownReadOnlyProperty<ErrorFlags>(new []{category, "empty_reason"});
			Format = new WellKnownReadOnlyProperty<ILogFileFormat>(new []{category, "format"});
			FormatDetectionCertainty = new WellKnownReadOnlyProperty<Certainty>(new []{category, "format_detection_certainty"});

			Minimum = new IReadOnlyPropertyDescriptor[]
			{
				LogEntryCount,
				Name,
				StartTimestamp,
				EndTimestamp,
				Duration,
				LastModified,
				Created,
				Size,
				PercentageProcessed,
				EmptyReason,
				Format,
				FormatDetectionCertainty
			};
		}

		/// <summary>
		/// </summary>
		/// <param name="additionalProperties"></param>
		/// <returns></returns>
		public static IReadOnlyList<IReadOnlyPropertyDescriptor> CombineWithMinimum(IEnumerable<IReadOnlyPropertyDescriptor> additionalProperties)
		{
			return Combine(Minimum, additionalProperties);
		}

		/// <summary>
		/// </summary>
		/// <param name="additionalProperties"></param>
		/// <returns></returns>
		public static IReadOnlyList<IReadOnlyPropertyDescriptor> CombineWithMinimum(params IReadOnlyPropertyDescriptor[] additionalProperties)
		{
			return Combine(Minimum, additionalProperties);
		}

		/// <summary>
		/// </summary>
		/// <param name="properties"></param>
		/// <param name="additionalProperties"></param>
		/// <returns></returns>
		public static IReadOnlyList<IReadOnlyPropertyDescriptor> Combine(IEnumerable<IReadOnlyPropertyDescriptor> properties,
		                                                                IEnumerable<IReadOnlyPropertyDescriptor>
			                                                                additionalProperties)
		{
			var allProperties = new List<IReadOnlyPropertyDescriptor>(properties);
			if (additionalProperties != null)
			{
				foreach (var property in additionalProperties)
					if (!allProperties.Contains(property))
						allProperties.Add(property);
			}
			return allProperties;
		}

		/// <summary>
		/// </summary>
		/// <param name="properties"></param>
		/// <param name="additionalProperties"></param>
		/// <returns></returns>
		public static IReadOnlyList<IReadOnlyPropertyDescriptor> Combine(IEnumerable<IReadOnlyPropertyDescriptor> properties,
		                                                                params IReadOnlyPropertyDescriptor[]
			                                                                additionalProperties)
		{
			return Combine(properties, (IEnumerable<IReadOnlyPropertyDescriptor>) additionalProperties);
		}
	}
}