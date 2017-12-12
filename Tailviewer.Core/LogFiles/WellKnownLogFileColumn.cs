using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     A column which is well-known by Tailviewer, i.e. one that can be interpreted
	///     because its meaning is understood (such as Timestamp, etc...).
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class WellKnownLogFileColumn<T>
		: ILogFileColumn<T>
	{
		private readonly string _name;
		private readonly string _id;

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		public WellKnownLogFileColumn(string id, string name)
		{
			if (id == null)
				throw new ArgumentNullException(nameof(id));
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			_id = id;
			_name = name;
		}

		/// <inheritdoc />
		public string Id => _id;

		/// <inheritdoc />
		public string Name => _name;

		/// <inheritdoc />
		public Type DataType => typeof(T);

		/// <inheritdoc />
		public override string ToString()
		{
			return _id;
		}
	}
}