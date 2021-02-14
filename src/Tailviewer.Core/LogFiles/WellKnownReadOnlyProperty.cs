using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     A read-only property which is well-known by Tailviewer, i.e. one that can be interpreted
	///     because its meaning is understood (such as StartTimestamp, etc...).
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class WellKnownReadOnlyProperty<T>
		: IReadOnlyPropertyDescriptor<T>
	{
		private readonly string _id;
		private readonly string _displayName;
		private readonly T _defaultValue;

		public WellKnownReadOnlyProperty(string id, string displayName = null, T defaultValue = default)
			: this(new []{id}, displayName, defaultValue)
		{}

		public WellKnownReadOnlyProperty(IEnumerable<string> path, string displayName = null, T defaultValue = default)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (path.Any(string.IsNullOrEmpty))
				throw new ArgumentException();

			_id = string.Join(".", path);
			_displayName = displayName;
			_defaultValue = defaultValue;
		}

		public string Id => _id;

		public string DisplayName => _displayName;

		public Type DataType => typeof(T);

		public T DefaultValue => _defaultValue;

		object IReadOnlyPropertyDescriptor.DefaultValue => DefaultValue;

		public override string ToString()
		{
			return string.Format("{0}: {1}", Id, DataType.Name);
		}
	}
}