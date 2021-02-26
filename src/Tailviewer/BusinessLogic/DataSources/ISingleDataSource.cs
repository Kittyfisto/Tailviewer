namespace Tailviewer.BusinessLogic.DataSources
{
	/// <summary>
	///     Tag interface for an <see cref="IDataSource"/> which consists of a single, unspecified source.
	/// </summary>
	/// <remarks>
	///     A class implementing this interface may additionally implement other tag interfaces to specify which
	///     kind of data source it is.
	/// </remarks>
	public interface ISingleDataSource
		: IDataSource
	{
	}
}