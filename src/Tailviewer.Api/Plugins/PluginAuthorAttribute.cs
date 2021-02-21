using System;

namespace Tailviewer.Plugins
{
	/// <summary>
	///     This attribute should be placed alongside other attributes in your
	///     AssemblyInfo.cs file. The author's name will be displayed to users.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class PluginAuthorAttribute
		: Attribute
	{
		/// <summary>
		///     Initializes this attribute with the given author.
		/// </summary>
		/// <param name="author"></param>
		public PluginAuthorAttribute(string author)
		{
			Author = author;
		}

		/// <summary>
		///     The author that will be displayed to users.
		/// </summary>
		public string Author { get; set; }
	}
}