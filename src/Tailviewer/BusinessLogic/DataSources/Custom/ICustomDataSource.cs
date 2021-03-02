using Tailviewer.Api;

namespace Tailviewer.BusinessLogic.DataSources.Custom
{
	/// <summary>
	///     The interface for a custom data source, e.g. one that is integrated into tailviewer via a plugin.
	/// </summary>
	public interface ICustomDataSource
		: ISingleDataSource
	{
		ICustomDataSourceConfiguration Configuration { get; }
	}
}