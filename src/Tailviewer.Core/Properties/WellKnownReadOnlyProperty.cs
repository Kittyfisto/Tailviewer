using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Api;

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
		private readonly T _defaultValue;
		private readonly string _id;

		public WellKnownReadOnlyProperty(string id, T defaultValue = default)
			: this(new[] {id}, defaultValue)
		{
		}

		public WellKnownReadOnlyProperty(IEnumerable<string> path, T defaultValue = default)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (path.Any(string.IsNullOrEmpty))
				throw new ArgumentException();

			_id = string.Join(".", path);
			_defaultValue = defaultValue;
		}

		public string Id
		{
			get { return _id; }
		}

		public Type DataType
		{
			get { return typeof(T); }
		}

		public T DefaultValue
		{
			get { return _defaultValue; }
		}

		IEqualityComparer<T> IReadOnlyPropertyDescriptor<T>.Comparer
		{
			get { return null; }
		}

		IEqualityComparer IReadOnlyPropertyDescriptor.Comparer
		{
			get { return null; }
		}

		object IReadOnlyPropertyDescriptor.DefaultValue
		{
			get { return DefaultValue; }
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", Id, DataType.Name);
		}
	}
}