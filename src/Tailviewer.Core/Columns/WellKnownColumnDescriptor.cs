using System;

namespace Tailviewer.Core.Columns
{
	/// <summary>
	///     A column which is well-known by Tailviewer, i.e. one that can be interpreted
	///     because its meaning is understood (such as Timestamp, etc...).
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class WellKnownColumnDescriptor<T>
		: IColumnDescriptor<T>
	{
		private readonly string _id;
		private readonly T _defaultValue;

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		public WellKnownColumnDescriptor(string id)
			: this(id, default)
		{}

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="defaultValue"></param>
		public WellKnownColumnDescriptor(string id, T defaultValue)
		{
			_id = id ?? throw new ArgumentNullException(nameof(id));
			_defaultValue = defaultValue;
		}

		/// <inheritdoc />
		public string Id => _id;

		/// <inheritdoc />
		public Type DataType => typeof(T);

		/// <inheritdoc />
		public T DefaultValue => _defaultValue;

		object IColumnDescriptor.DefaultValue => DefaultValue;

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("{0}: {1}", _id, typeof(T).Name);
		}
	}
}