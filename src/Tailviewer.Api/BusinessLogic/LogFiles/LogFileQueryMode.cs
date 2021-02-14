namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	/// 
	/// </summary>
	public enum LogFileQueryMode
	{
		/// <summary>
		///    If the data is known to exist (e.g. the requested entry is truly part of the log file),
		///    then the calling thread is blocked until the data becomes available.
		///    If the data doesn't exist, then the destination is filled with the respective column's
		///    default value.
		/// </summary>
		FromSource,

		/// <summary>
		///    If the data is readily available, then it is copied to the destination.
		///    Otherwise the destination is filled with the respective column's default value.
		///    The client may try to query the data again at a later point in time and the <see cref="ILogFile"/>
		///    implementation is encouraged to retrieve the data asynchronously in the meantime, if possible.
		/// </summary>
		FromCacheOnly
	}
}