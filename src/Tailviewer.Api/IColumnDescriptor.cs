using System;

namespace Tailviewer.Api
{
	/// <summary>
	///     Describes a column of a log file.
	/// </summary>
	/// <remarks>
	///     TODO: Introduce required EqualityComparer
	///     TODO: Introduce optional Comparer (for sorting)
	/// </remarks>
	public interface IColumnDescriptor
	{
		/// <summary>
		///     Id of this column, two columns are the same if they have the same id.
		/// </summary>
		string Id { get; }

		/// <summary>
		///     The type of the data provided by this column.
		/// </summary>
		Type DataType { get; }

		/// <summary>
		///     The value used when an invalid row is accessed or
		///     when no value is available.
		/// </summary>
		object DefaultValue { get; }
	}

	/// <summary>
	///     Describes a column of a log file.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IColumnDescriptor<out T>
		: IColumnDescriptor
	{
		/// <summary>
		///     The value used when an invalid row is accessed or
		///     when no value is available.
		/// </summary>
		new T DefaultValue { get; }
	}
}