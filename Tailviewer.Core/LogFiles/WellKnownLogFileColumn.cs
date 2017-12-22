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
		private readonly string _id;
		private readonly T _defaultValue;

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		public WellKnownLogFileColumn(string id)
			: this(id, default(T))
		{}

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="defaultValue"></param>
		public WellKnownLogFileColumn(string id, T defaultValue)
		{
			if (id == null)
				throw new ArgumentNullException(nameof(id));

			_id = id;
			_defaultValue = defaultValue;
		}

		/// <inheritdoc />
		public string Id => _id;

		/// <inheritdoc />
		public Type DataType => typeof(T);

		/// <inheritdoc />
		public T DefaultValue => _defaultValue;

		object ILogFileColumn.DefaultValue => DefaultValue;

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("{0}: {1}", _id, typeof(T).Name);
		}
	}
}