using System;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Describes a column of a log file.
	/// </summary>
	public interface ILogFileColumn
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
	public interface ILogFileColumn<out T>
		: ILogFileColumn
	{
		/// <summary>
		///     The value used when an invalid row is accessed or
		///     when no value is available.
		/// </summary>
		new T DefaultValue { get; }
	}
}