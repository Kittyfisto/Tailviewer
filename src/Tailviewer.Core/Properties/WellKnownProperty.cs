using System.Collections.Generic;

namespace Tailviewer.Core.Properties
{
	/// <summary>
	///     A property which is well-known by Tailviewer and can be changed by the user.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class WellKnownProperty<T>
		: WellKnownReadOnlyProperty<T>
		, IPropertyDescriptor<T>
	{
		public WellKnownProperty(string id, T defaultValue = default)
			: base(new []{id}, defaultValue)
		{}

		public WellKnownProperty(IEnumerable<string> path, T defaultValue = default)
			: base(path, defaultValue)
		{}
	}
}