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
		///     The minimum set of properties a log file is expected to provide.
		/// </summary>
		public static readonly IReadOnlyList<ILogFilePropertyDescriptor> Minimum;

		static LogFileProperties()
		{
			Name = new WellKnownLogFilePropertyDescriptor<string>("Name");
			StartTimestamp = new WellKnownLogFilePropertyDescriptor<DateTime?>("StartTimestamp");
			EndTimestamp = new WellKnownLogFilePropertyDescriptor<DateTime?>("EndTimestamp");
			Duration = new WellKnownLogFilePropertyDescriptor<TimeSpan?>("Duration");
			LastModified = new WellKnownLogFilePropertyDescriptor<DateTime?>("LastModified");
			Created = new WellKnownLogFilePropertyDescriptor<DateTime?>("Created");
			Size = new WellKnownLogFilePropertyDescriptor<Size?>("Size");
			EmptyReason = new WellKnownLogFilePropertyDescriptor<ErrorFlags>("EmptyReason");
			Format = new WellKnownLogFilePropertyDescriptor<ILogFileFormat>("Format");
			FormatDetectionCertainty = new WellKnownLogFilePropertyDescriptor<Certainty>("FormatDetectionCertainty");
			Encoding = new WellKnownLogFilePropertyDescriptor<Encoding>("Encoding");

			Minimum = new ILogFilePropertyDescriptor[]
			{
				Name,
				StartTimestamp,
				EndTimestamp,
				Duration,
				LastModified,
				Created,
				Size,
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