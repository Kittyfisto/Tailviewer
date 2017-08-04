using System;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     This attribute should be placed alongside other attributes in your
	///     AssemblyInfo.cs file. The plugin id will be used internally to determine
	///     if two versions of the same plugin are present (versus two plugins which coincidentally
	///     share the same name).
	///     The id will not be visible to the user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class PluginIdAttribute
		: Attribute
	{
		/// <summary>
		///     Initializes this attribute with the given id.
		/// </summary>
		/// <param name="id"></param>
		public PluginIdAttribute(string id)
		{
			Id = id;
		}

		/// <summary>
		///     The author that will be displayed to users.
		/// </summary>
		public string Id { get; set; }
	}
}