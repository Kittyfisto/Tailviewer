using System;

namespace Tailviewer.Plugins
{
	/// <summary>
	///     This attribute should be placed alongside other attributes in your
	///     AssemblyInfo.cs file. The plugin id will be used internally to determine
	///     if two versions of the same plugin are present (versus two plugins which coincidentally
	///     share the same name).
	///     The id will not be visible to the user.
	/// </summary>
	/// <example>
	///     Suppose you're working for a company named "Strawberry" in a team named "Rocketeers" and
	///     you're developing a plugin to read your custom log format.
	///
	///     A good id would be:
	///         [PluginId("Strawberry.Rocketeers", "Log")]
	/// </example>
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class PluginIdAttribute
		: Attribute
	{
		/// <summary>
		///     Initializes this attribute with the given id.
		/// </summary>
		/// <param name="namespace"></param>
		/// <param name="name"></param>
		public PluginIdAttribute(string @namespace, string name)
		{
			Namespace = @namespace;
			Name = name;
		}

		/// <summary>
		/// The namespace-part of the plugin id. This could be your online handle,
		/// the name of your company, etc...
		/// </summary>
		public string Namespace { get; set; }

		/// <summary>
		/// The name-part of the plugin id.
		/// </summary>
		public string Name { get; set; }
	}
}