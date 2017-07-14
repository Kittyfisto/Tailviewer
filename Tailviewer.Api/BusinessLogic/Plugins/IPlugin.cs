using System;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     The base interface for any plugin.
	///     Should be implemented to provide the user with information about
	///     the origin of a plugin.
	/// </summary>
	public interface IPlugin
	{
		/// <summary>
		///     The author of the plugin.
		///     Will be displayed to the end-user if the inspects the list of plugins.
		/// </summary>
		string Author { get; }

		/// <summary>
		///     The website of the plugin (for example where the plugin can be downloaded,
		///     its github page, etc...).
		/// </summary>
		Uri Website { get; }
	}
}