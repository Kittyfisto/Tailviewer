using System;
using System.Text;

namespace Tailviewer
{
	/// <summary>
	///     Describes how data is to be retrieved from an <see cref="ILogSource" />.
	/// </summary>
	public sealed class LogFileQueryOptions
	{
		/// <summary>
		///     The default query options, <see cref="LogFileQueryMode.FromSource" />.
		/// </summary>
		public static readonly LogFileQueryOptions Default;

		/// <summary>
		///     The maximum time the log file shall block in case <see cref="QueryMode" /> is set to
		///     <see cref="LogFileQueryMode.FromSource" />.
		///     Ignored otherwise.
		/// </summary>
		public TimeSpan MaximumWaitTime;

		/// <summary>
		///     How the log file shall block, if at all.
		/// </summary>
		public LogFileQueryMode QueryMode;

		static LogFileQueryOptions()
		{
			Default = new LogFileQueryOptions(LogFileQueryMode.FromSource)
			{
				MaximumWaitTime = TimeSpan.MaxValue
			};
		}

		/// <summary>
		/// </summary>
		/// <param name="queryMode"></param>
		public LogFileQueryOptions(LogFileQueryMode queryMode)
		{
			QueryMode = queryMode;
		}

		#region Overrides of Object

		/// <inheritdoc />
		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.Append(QueryMode);
			if (QueryMode == LogFileQueryMode.FromSource)
			{
				builder.AppendFormat("Wait: {0}", MaximumWaitTime);
			}
			return builder.ToString();
		}

		#endregion

		#region Equality members

		private bool Equals(LogFileQueryOptions other)
		{
			return QueryMode == other.QueryMode &&
			       MaximumWaitTime.Equals(other.MaximumWaitTime);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is LogFileQueryOptions other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return ((int) QueryMode * 397) ^ MaximumWaitTime.GetHashCode();
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(LogFileQueryOptions left, LogFileQueryOptions right)
		{
			return Equals(left, right);
		}

		/// <summary>
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(LogFileQueryOptions left, LogFileQueryOptions right)
		{
			return !Equals(left, right);
		}

		#endregion
	}
}