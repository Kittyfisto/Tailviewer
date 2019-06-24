namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Fully represents another <see cref="ILogFile" /> which can be replaced over the lifetime
	///     of the proxy.
	/// </summary>
	/// <remarks>
	///     Exists so that specialized <see cref="ILogFile" /> implementations don't need to be concerned about re-use
	///     or certain changes.
	/// </remarks>
	public interface ILogFileProxy
		: ILogFile
	{
		/// <summary>
		///     The log file represented by this proxy.
		/// </summary>
		ILogFile InnerLogFile { get; set; }
	}
}