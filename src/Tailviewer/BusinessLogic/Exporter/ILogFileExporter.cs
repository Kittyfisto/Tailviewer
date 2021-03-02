using System;
using Tailviewer.Api;

namespace Tailviewer.BusinessLogic.Exporter
{
	public interface ILogFileExporter
	{
		void Export(IProgress<Percentage> progressReporter = null);
	}
}