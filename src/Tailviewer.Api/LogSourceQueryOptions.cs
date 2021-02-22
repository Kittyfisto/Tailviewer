using System;
using System.Text;

namespace Tailviewer
{
	/// <summary>
	///     Describes how data is to be retrieved from an <see cref="ILogSource" />.
	/// </summary>
	public sealed class LogSourceQueryOptions
	{
		/// <summary>
		///     The default query options, <see cref="LogSourceQueryMode.Default" />.
		/// </summary>
		public static readonly LogSourceQueryOptions Default;

		/// <summary>
		///     The maximum time the log file shall block in case <see cref="QueryMode" /> is set to
		///     <see cref="LogSourceQueryMode.Default" />.
		///     Ignored otherwise.
		/// </summary>
		public readonly TimeSpan MaximumWaitTime;

		/// <summary>
		///     How the log file shall block, if at all.
		/// </summary>
		public readonly LogSourceQueryMode QueryMode;

		static LogSourceQueryOptions()
		{
			Default = new LogSourceQueryOptions(LogSourceQueryMode.Default, TimeSpan.MaxValue);
		}

		/// <summary>
		/// </summary>
		/// <param name="queryMode"></param>
		public LogSourceQueryOptions(LogSourceQueryMode queryMode)
			: this(queryMode, TimeSpan.MaxValue)
		{}

		/// <summary>
		/// </summary>
		/// <param name="queryMode"></param>
		/// <param name="maximumWaitTime"></param>
		public LogSourceQueryOptions(LogSourceQueryMode queryMode, TimeSpan maximumWaitTime)
		{
			QueryMode = queryMode;
			MaximumWaitTime = maximumWaitTime;
		}

		#region Overrides of Object

		/// <inheritdoc />
		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.Append(QueryMode);
			if (QueryMode == LogSourceQueryMode.Default)
			{
				builder.AppendFormat("Wait: {0}", MaximumWaitTime);
			}
			return builder.ToString();
		}

		#endregion

		#region Equality members

		private bool Equals(LogSourceQueryOptions other)
		{
			return QueryMode == other.QueryMode &&
			       MaximumWaitTime.Equals(other.MaximumWaitTime);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is LogSourceQueryOptions other && Equals(other);
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
		public static bool operator ==(LogSourceQueryOptions left, LogSourceQueryOptions right)
		{
			return Equals(left, right);
		}

		/// <summary>
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(LogSourceQueryOptions left, LogSourceQueryOptions right)
		{
			return !Equals(left, right);
		}

		#endregion
	}
}