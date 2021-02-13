using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     A column which is not known by Tailviewer, i.e. one that cannot be interpreted and merely
	///     be forwarded and eventually displayed to the user.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class CustomLogFileColumnDescriptor<T>
		: ILogFileColumnDescriptor<T>
	{
		private readonly string _id;
		private readonly string _displayName;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="id">Identifies this column amongst all other columns: Two columns are equal if their id is</param>
		public CustomLogFileColumnDescriptor(string id)
			: this(id, id)
		{}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="id">Identifies this column amongst all other columns: Two columns are equal if their id is</param>
		/// <param name="displayName">The human readable name of this column</param>
		public CustomLogFileColumnDescriptor(string id, string displayName)
		{
			_id = id ?? throw new ArgumentNullException(nameof(id));
			_displayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
		}

		/// <inheritdoc />
		public string Id => _id;

		/// <inheritdoc />
		public string DisplayName => _displayName;

		/// <inheritdoc />
		public Type DataType => typeof(T);

		/// <inheritdoc />
		public T DefaultValue => default(T);

		object ILogFileColumnDescriptor.DefaultValue => DefaultValue;

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			var other = obj as CustomLogFileColumnDescriptor<T>;
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