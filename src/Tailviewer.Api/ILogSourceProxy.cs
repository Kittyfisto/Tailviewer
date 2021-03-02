namespace Tailviewer.Api
{
	/// <summary>
	///     Fully represents another <see cref="ILogSource" /> which can be replaced over the lifetime
	///     of the proxy.
	/// </summary>
	/// <remarks>
	///     Exists so that specialized <see cref="ILogSource" /> implementations don't need to be concerned about re-use
	///     or certain changes.
	/// </remarks>
	public interface ILogSourceProxy
		: ILogSource
	{
		/// <summary>
		///     The log file represented by this proxy.
		/// </summary>
		ILogSource InnerLogSource { get; set; }
	}
}