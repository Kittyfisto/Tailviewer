namespace Tailviewer.Core.Settings
{
	/// <summary>
	/// </summary>
	public enum TimeFilterMode
	{
		/// <summary>
		///     Do not filter by time (i.e. every log event passes the filter).
		/// </summary>
		Everything,

		/// <summary>
		///     Filter using the specified special interval (e.g. today, this week, etc...)
		/// </summary>
		SpecialInterval,

		/// <summary>
		///     Filter using the specified custom interval (e.g. from A to B)
		/// </summary>
		Interval
	}
}