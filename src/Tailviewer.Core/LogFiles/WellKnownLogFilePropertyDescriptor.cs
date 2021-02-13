using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     A property which is well-known by Tailviewer, i.e. one that can be interpreted
	///     because its meaning is understood (such as StartTimestamp, etc...).
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class WellKnownLogFilePropertyDescriptor<T>
		: ILogFilePropertyDescriptor<T>
	{
		private readonly string[] _path;
		private string _displayName;
		private readonly T _defaultValue;

		public WellKnownLogFilePropertyDescriptor(string id)
			: this(id, id, default(T))
		{}

		public WellKnownLogFilePropertyDescriptor(string id, T defaultValue)
			: this(new []{id}, id, defaultValue)
		{}

		public WellKnownLogFilePropertyDescriptor(string id, string displayName)
			: this(id, displayName, default(T))
		{}

		public WellKnownLogFilePropertyDescriptor(string id, string displayName, T defaultValue)
			: this(new []{id}, displayName, defaultValue)
		{}

		public WellKnownLogFilePropertyDescriptor(IEnumerable<string> path, string displayName)
			: this(path, displayName, default)
		{ }

		public WellKnownLogFilePropertyDescriptor(IEnumerable<string> path, string displayName, T defaultValue)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (path.Any(string.IsNullOrEmpty))
				throw new ArgumentException();

			_path = path.ToArray();
			_displayName = displayName;
			_defaultValue = defaultValue;
		}

		public string Id => string.Join(".", _path);

		public string DisplayName => _displayName;

		public IReadOnlyList<string> Path => _path;

		public Type DataType => typeof(T);

		public T DefaultValue => _defaultValue;

		object ILogFilePropertyDescriptor.DefaultValue => DefaultValue;

		public override string ToString()
		{
			return string.Format("{0}: {1}", Id, DataType.Name);
		}
	}
}