using System;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     This attribute should be placed alongside other attributes in your
	///     AssemblyInfo.cs file. The uri will be displayed to users.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class PluginWebsiteAttribute
		: Attribute
	{
		/// <summary>
		///     Initializes this attribute.
		/// </summary>
		/// <param name="website"></param>
		public PluginWebsiteAttribute(string website)
		{
			Website = new Uri(website, UriKind.RelativeOrAbsolute);
		}

		/// <summary>
		///     The website a user should go to to find more information about this plugin.
		/// </summary>
		public Uri Website { get; set; }
	}
}