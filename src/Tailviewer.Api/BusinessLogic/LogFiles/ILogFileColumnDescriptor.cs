using System;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Describes a column of a log file.
	/// </summary>
	public interface ILogFileColumnDescriptor
	{
		/// <summary>
		///     Id of this column, two columns are the same if they have the same id.
		/// </summary>
		string Id { get; }

		/// <summary>
		///     The human readable name of this column. If none is given, then <see cref="Id"/> is used.
		/// </summary>
		string DisplayName { get; }

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
	public interface ILogFileColumnDescriptor<out T>
		: ILogFileColumnDescriptor
	{
		/// <summary>
		///     The value used when an invalid row is accessed or
		///     when no value is available.
		/// </summary>
		new T DefaultValue { get; }
	}
}