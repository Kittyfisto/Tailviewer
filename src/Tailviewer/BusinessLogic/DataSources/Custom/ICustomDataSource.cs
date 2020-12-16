using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.BusinessLogic.DataSources.Custom
{
	public interface ICustomDataSource
		: IDataSource
	{
		ICustomDataSourceConfiguration Configuration { get; }
	}
}