using Tailviewer.Plugins;

namespace Tailviewer.Ui
{
	/// <summary>
	///    This plugin is responsible for creating <see cref="IPropertyPresenter"/>s for <see cref="IPropertyDescriptor"/>s.
	///    With this, plugins may introduce their own properties and write their own presenters which dictate how they are shown
	///    to the user as well as how the user may change them.
	/// </summary>
	/// <remarks>
	///     Version 1 (2021-02-14)
	///     Initial definition
	/// </remarks>
	[PluginInterfaceVersion(version: 1)]
	public interface IPropertyPresenterPlugin
		: IPlugin
	{
		/// <summary>
		///    Tries to create a presenter for the given property of the given log file, if possible.
		/// </summary>
		/// <remarks>
		///    This method shouldn't throw an exception when it doesn't know the property and instead return null.
		/// </remarks>
		/// <param name="property"></param>
		/// <returns>The presenter for the property or null in case this plugin doesn't have a presenter for this property</returns>
		IPropertyPresenter TryCreatePresenterFor(IReadOnlyPropertyDescriptor property);
	}
}