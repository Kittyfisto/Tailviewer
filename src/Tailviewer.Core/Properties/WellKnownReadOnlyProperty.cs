using System;
using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.Core.Properties
{
	/// <summary>
	///     A read-only property which is well-known by Tailviewer, i.e. one that can be interpreted
	///     because its meaning is understood (such as StartTimestamp, etc...).
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class WellKnownReadOnlyProperty<T>
		: IReadOnlyPropertyDescriptor<T>
		, IWellKnownReadOnlyPropertyDescriptor
	{
		private readonly string _id;
		private readonly T _defaultValue;

		public WellKnownReadOnlyProperty(string id, T defaultValue = default)
			: this(new []{id}, defaultValue)
		{}

		public WellKnownReadOnlyProperty(IEnumerable<string> path, T defaultValue = default)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (path.Any(string.IsNullOrEmpty))
				throw new ArgumentException();

			_id = string.Join(".", path);
			_defaultValue = defaultValue;
		}

		public string Id => _id;

		public Type DataType => typeof(T);

		public T DefaultValue => _defaultValue;

		object IReadOnlyPropertyDescriptor.DefaultValue => DefaultValue;

		public override string ToString()
		{
			return string.Format("{0}: {1}", Id, DataType.Name);
		}
	}
}