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
		private readonly T _defaultValue;

		public WellKnownLogFilePropertyDescriptor(string id)
			: this(id, default(T))
		{}

		public WellKnownLogFilePropertyDescriptor(string id, T defaultValue)
			: this(new []{id}, defaultValue)
		{}

		public WellKnownLogFilePropertyDescriptor(params string[] path)
			: this(path, default(T))
		{}

		public WellKnownLogFilePropertyDescriptor(IEnumerable<string> path, T defaultValue)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (path.Any(string.IsNullOrEmpty))
				throw new ArgumentException();

			_path = path.ToArray();
			_defaultValue = defaultValue;
		}

		public string Id => string.Join(".", _path);

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