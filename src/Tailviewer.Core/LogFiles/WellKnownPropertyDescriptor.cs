using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     A property which is well-known by Tailviewer and can be changed by the user.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class WellKnownPropertyDescriptor<T>
		: WellKnownReadOnlyProperty<T>
		, IPropertyDescriptor<T>
	{
		public WellKnownPropertyDescriptor(string id, T defaultValue = default)
			: base(new []{id}, defaultValue)
		{}

		public WellKnownPropertyDescriptor(IEnumerable<string> path, T defaultValue = default)
			: base(path, defaultValue)
		{}
	}
}