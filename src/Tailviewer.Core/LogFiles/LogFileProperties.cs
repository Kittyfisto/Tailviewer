using System;
using System.Collections.Generic;
using System.Text;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Maintains a collection of well-known log file properties.
	/// </summary>
	public static class LogFileProperties
	{
		/// <summary>
		///     The number of log entries in the log file.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<int> LogEntryCount;

		/// <summary>
		///     The name of the log file (for example the file name of a text log file).
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<string> Name;

		/// <summary>
		///     The first identified timestamp of the data source, if any, null otherwise.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<DateTime?> StartTimestamp;

		/// <summary>
		///     The last identified timestamp of the data source, if any, null otherwise.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<DateTime?> EndTimestamp;

		/// <summary>
		///     The difference between the first and last timestamp.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<TimeSpan?> Duration;

		/// <summary>
		///     The timestamp (in local time) the data source has last been modified.
		///     A modification is meant to be the addition and/or removal of at least one log line.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<DateTime?> LastModified;

		/// <summary>
		///     The timestamp (in local time) the data source has been created.
		/// </summary>
		/// <remarks>
		///     Is set to null when the data source doesn't exist.
		/// </remarks>
		public static readonly ILogFilePropertyDescriptor<DateTime?> Created;

		/// <summary>
		///     The approximate size of the data source.
		///     Is only needed to be displayed to the user.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<Size?> Size;

		/// <summary>
		///     The percentage of the file which has been fully processed already.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<Percentage> PercentageProcessed;

		/// <summary>
		///     The error, if any, which describes why this log file is empty.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<ErrorFlags> EmptyReason;

		/// <summary>
		///     The format of the log file, as determined by a <see cref="ILogFileFormatMatcher"/>.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<ILogFileFormat> Format;

		/// <summary>
		///     The certainty with which <see cref="ILogFileFormatMatcher"/> has detected the format.
		///     It is possible that while tailing a file, the format may change in case another format
		///     is now a better match.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<Certainty> FormatDetectionCertainty;

		/// <summary>
		///     The <see cref="Encoding"/> used by tailviewer to interpret the log file's content as text.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<Encoding> Encoding;

		/// <summary>
		///     When set to true, then the log file has processed its entire source.
		/// </summary>
		/// <remarks>
		///     Internal property only used for testing.
		/// </remarks>
		internal static readonly ILogFilePropertyDescriptor<bool> EndOfSourceReached;

		/// <summary>
		///     The minimum set of properties a log file is expected to provide.
		/// </summary>
		public static readonly IReadOnlyList<ILogFilePropertyDescriptor> Minimum;

		static LogFileProperties()
		{
			var category = "general";

			LogEntryCount = new WellKnownLogFilePropertyDescriptor<int>(new[] {category, "log_entry_count"});
			Name = new WellKnownLogFilePropertyDescriptor<string>(new []{category, "name"}, "Name");
			StartTimestamp = new WellKnownLogFilePropertyDescriptor<DateTime?>(new []{category, "start_timestamp"}, "Start Time");
			EndTimestamp = new WellKnownLogFilePropertyDescriptor<DateTime?>(new []{category, "end_timestamp"}, "End Time");
			Duration = new WellKnownLogFilePropertyDescriptor<TimeSpan?>(new []{category, "duration"}, "Duration");
			LastModified = new WellKnownLogFilePropertyDescriptor<DateTime?>(new []{category, "last_modified"}, "Last Modified");
			Created = new WellKnownLogFilePropertyDescriptor<DateTime?>(new []{category, "created"}, "Created");
			Size = new WellKnownLogFilePropertyDescriptor<Size?>(new []{category, "size"}, "Size");

			PercentageProcessed = new WellKnownLogFilePropertyDescriptor<Percentage>(new []{category, "percentage_processed"}, "Processed", Percentage.Zero);
			EmptyReason = new WellKnownLogFilePropertyDescriptor<ErrorFlags>(new []{category, "empty_reason"}, "Empty");
			Format = new WellKnownLogFilePropertyDescriptor<ILogFileFormat>(new []{category, "format"}, "Format");
			FormatDetectionCertainty = new WellKnownLogFilePropertyDescriptor<Certainty>(new []{category, "FormatDetectionCertainty"});
			Encoding = new WellKnownLogFilePropertyDescriptor<Encoding>(new []{category, "encoding"}, "Encoding");

			EndOfSourceReached = new WellKnownLogFilePropertyDescriptor<bool>(new[] {"end_of_source_reached"});

			Minimum = new ILogFilePropertyDescriptor[]
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
				FormatDetectionCertainty,
				Encoding
			};
		}

		/// <summary>
		/// </summary>
		/// <param name="additionalProperties"></param>
		/// <returns></returns>
		public static IReadOnlyList<ILogFilePropertyDescriptor> CombineWithMinimum(IEnumerable<ILogFilePropertyDescriptor> additionalProperties)
		{
			return Combine(Minimum, additionalProperties);
		}

		/// <summary>
		/// </summary>
		/// <param name="additionalProperties"></param>
		/// <returns></returns>
		public static IReadOnlyList<ILogFilePropertyDescriptor> CombineWithMinimum(params ILogFilePropertyDescriptor[] additionalProperties)
		{
			return Combine(Minimum, additionalProperties);
		}

		/// <summary>
		/// </summary>
		/// <param name="properties"></param>
		/// <param name="additionalProperties"></param>
		/// <returns></returns>
		public static IReadOnlyList<ILogFilePropertyDescriptor> Combine(IEnumerable<ILogFilePropertyDescriptor> properties,
		                                                                IEnumerable<ILogFilePropertyDescriptor>
			                                                                additionalProperties)
		{
			var allProperties = new List<ILogFilePropertyDescriptor>(properties);
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
		public static IReadOnlyList<ILogFilePropertyDescriptor> Combine(IEnumerable<ILogFilePropertyDescriptor> properties,
		                                                                params ILogFilePropertyDescriptor[]
			                                                                additionalProperties)
		{
			return Combine(properties, (IEnumerable<ILogFilePropertyDescriptor>) additionalProperties);
		}
	}
}