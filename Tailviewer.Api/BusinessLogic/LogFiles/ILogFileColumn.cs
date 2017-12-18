using System;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
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
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ILogFileColumn<T>
		: ILogFileColumn
	{
	}
}