using System;

namespace Tailviewer.Api
{
	/// <summary>
	///    Controls how data is supposed to be retrieved from a source.
	///    In 99% of all programmatic use cases, <see cref="Default"/> will do.
	/// </summary>
	[Flags]
	public enum LogSourceQueryMode
	{
		/// <summary>
		///    If the data is known to exist (e.g. the requested entry is truly part of the log file),
		///    then the calling thread is blocked until the data becomes available.
		///    If the data doesn't exist, then the destination is filled with the respective column's
		///    default value.
		/// </summary>
		/// <remarks>
		///    The reasoning for having the default be don't cache is that I'm afraid another setting would
		///    starve out the ui thread. Once background threads start hammering a log file in order to
		///    count lines and do whatever, then we stand a chance that these threads will *always* manage
		///    to fill the cache with data the user isn't currently viewing on screen. It is likely that given
		///    a certain cache size and activity, that the entire cache is filled with whatever portions are currently
		///    scanned, completely drowning out the most important thing: The center of the screen.
		/// </remarks>
		Default = FromCache | FromSource | DontCache,

		/// <summary>
		///    The prevalent option for the UI where we want:
		///    - to have a low latency (=> fetch from cache only)
		///    - are totally fine with only reading partially from cache (the UI will try again later if that's the case)
		///    - are happy if the cache were to try to fetch from the source anyways, but just from a BG thread please
		/// </summary>
		Display = FromCache | FetchForLater,


		/// <summary>
		///    If the data is readily available in the cache, then it is copied to the destination.
		/// </summary>
		/// <remarks>
		///    When this value is specified and <see cref="FromSource"/> is not, then the data returned might only be
		///    a subset of the source's actual data. This can be checked via the index column's value, which is set to invalid
		///    when the data couldn't be retrieved and by checking the number of log entries in the source.
		/// </remarks>
		FromCache = 0x01,

		/// <summary>
		///    Retrieve the data from the source.
		/// </summary>
		FromSource = 0x02,

		/// <summary>
		///    Don't store the data in the cache.
		/// </summary>
		DontCache = 0x04,

		/// <summary>
		///    The system should try to fetch the data asynchronously so that subsequent reads at a later date
		///    stand a greater chance of fetching the data.
		/// </summary>
		FetchForLater = 0x08,
	}
}