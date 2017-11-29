namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	/// </summary>
	public interface ILogFileColumn
	{
		/// <summary>
		///     Id of this column, two columns are the same if they have the same id.
		/// </summary>
		object Id { get; }

		/// <summary>
		///     Human readable name of this column.
		/// </summary>
		string Name { get; }
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ILogFileColumn<T>
		: ILogFileColumn
	{
	}
}