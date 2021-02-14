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
	public static class Properties
	{
		/// <summary>
		///     The number of log entries in the log file.
		/// </summary>
		public static readonly IReadOnlyPropertyDescriptor<int> LogEntryCount;

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
		///     The <see cref="Encoding"/> used by tailviewer to interpret the log file's content as text.
		/// </summary>
		public static readonly IPropertyDescriptor<Encoding> Encoding;

		/// <summary>
		///     The minimum set of properties a log file is expected to provide.
		/// </summary>
		public static readonly IReadOnlyList<IReadOnlyPropertyDescriptor> Minimum;

		static Properties()
		{
			var category = "general";

			LogEntryCount = new WellKnownReadOnlyProperty<int>(new[] {category, "log_entry_count"});
			Name = new WellKnownReadOnlyProperty<string>(new []{category, "name"}, "Name");
			StartTimestamp = new WellKnownReadOnlyProperty<DateTime?>(new []{category, "start_timestamp"}, "Start Time");
			EndTimestamp = new WellKnownReadOnlyProperty<DateTime?>(new []{category, "end_timestamp"}, "End Time");
			Duration = new WellKnownReadOnlyProperty<TimeSpan?>(new []{category, "duration"}, "Duration");
			LastModified = new WellKnownReadOnlyProperty<DateTime?>(new []{category, "last_modified"}, "Last Modified");
			Created = new WellKnownReadOnlyProperty<DateTime?>(new []{category, "created"}, "Created");
			Size = new WellKnownReadOnlyProperty<Size?>(new []{category, "size"}, "Size");

			PercentageProcessed = new WellKnownReadOnlyProperty<Percentage>(new []{category, "percentage_processed"}, "Processed", Percentage.Zero);
			EmptyReason = new WellKnownReadOnlyProperty<ErrorFlags>(new []{category, "empty_reason"}, "Empty");
			Format = new WellKnownReadOnlyProperty<ILogFileFormat>(new []{category, "format"}, "Format");
			FormatDetectionCertainty = new WellKnownReadOnlyProperty<Certainty>(new []{category, "FormatDetectionCertainty"});
			Encoding = new WellKnownPropertyDescriptor<Encoding>(new []{category, "encoding"}, "Encoding");

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
				FormatDetectionCertainty,
				Encoding
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