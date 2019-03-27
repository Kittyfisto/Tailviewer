using System;

namespace Tailviewer.Core.LogTables.Parsers
{
	/// <summary>
	///     The header of a column.
	/// </summary>
	public sealed class ColumnHeader
		: IColumnHeader
	{
		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="name"></param>
		public ColumnHeader(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
		}

		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public override string ToString()
		{
			return Name;
		}
	}
}