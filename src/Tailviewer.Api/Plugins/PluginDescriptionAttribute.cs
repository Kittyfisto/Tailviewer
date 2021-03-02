using System;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Api
{
	/// <summary>
	///     This attribute should be placed alongside other attributes in your
	///     AssemblyInfo.cs file. The description will be displayed to users.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class PluginDescriptionAttribute
		: Attribute
	{
		/// <summary>
		///     Initializes this attribute.
		/// </summary>
		/// <param name="description"></param>
		public PluginDescriptionAttribute(string description)
		{
			Description = description;
		}

		/// <summary>
		///     The website a user should go to to find more information about this plugin.
		/// </summary>
		public string Description { get; set; }
	}
}