using System;
using System.Collections.Generic;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Maintains a collection of well-known log file properties.
	/// </summary>
	public static class LogFileProperties
	{
		/// <summary>
		///     The first identified timestamp of the data source, if any, null otherwise.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<DateTime?> StartTime;

		/// <summary>
		///     The last identified timestamp of the data source, if any, null otherwise.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<DateTime?> EndTime;

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
		public static readonly ILogFilePropertyDescriptor<Size> Size;

		/// <summary>
		///     The error, if any, which describes why this log file is empty.
		/// </summary>
		public static readonly ILogFilePropertyDescriptor<ErrorFlags> EmptyReason;

		/// <summary>
		///     The minimum set of properties a log file is expected to provide.
		/// </summary>
		public static readonly IReadOnlyList<ILogFilePropertyDescriptor> Minimum;

		static LogFileProperties()
		{
			StartTime = new WellKnownLogFilePropertyDescriptor<DateTime?>("StartTime");
			EndTime = new WellKnownLogFilePropertyDescriptor<DateTime?>("EndTime");
			LastModified = new WellKnownLogFilePropertyDescriptor<DateTime?>("LastModified");
			Created = new WellKnownLogFilePropertyDescriptor<DateTime?>("Created");
			Size = new WellKnownLogFilePropertyDescriptor<Size>("Size");
			EmptyReason = new WellKnownLogFilePropertyDescriptor<ErrorFlags>("EmptyReason");

			Minimum = new ILogFilePropertyDescriptor[]
			{
				StartTime,
				EndTime,
				LastModified,
				Created,
				Size,
				EmptyReason
			};
		}
	}
}