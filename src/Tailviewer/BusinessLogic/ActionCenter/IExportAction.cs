using System;
using Tailviewer.Api;

namespace Tailviewer.BusinessLogic.ActionCenter
{
	public interface IExportAction
		: INotification
	{
		Exception Exception { get; }
		Percentage Progress { get; }
		string DataSourceName { get; }
		string FullExportFilename { get; }
		string Destination { get; }
	}
}