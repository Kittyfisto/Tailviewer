using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     A column which is not known by Tailviewer, i.e. one that cannot be interpreted and merely
	///     be forwarded and eventually displayed to the user.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class CustomLogFileColumn<T>
		: ILogFileColumn<T>
	{
		private readonly string _id;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="id">Identifies this column amongst all other columns: Two columns are equal if their id is</param>
		public CustomLogFileColumn(string id)
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
		public override bool Equals(object obj)
		{
			var other = obj as CustomLogFileColumn<T>;
			if (other == null)
				return false;

			return Equals(_id, other._id);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return _id.GetHashCode();
		}
	}
}