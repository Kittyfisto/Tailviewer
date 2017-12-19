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

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		public WellKnownLogFileColumn(string id)
		{
			if (id == null)
				throw new ArgumentNullException(nameof(id));

			_id = id;
		}

		/// <inheritdoc />
		public string Id => _id;

		/// <inheritdoc />
		public Type DataType => typeof(T);

		/// <inheritdoc />
		public T DefaultValue => default(T);

		object ILogFileColumn.DefaultValue => DefaultValue;

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("{0}: {1}", _id, typeof(T).Name);
		}
	}
}