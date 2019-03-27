using System;

namespace Tailviewer.BusinessLogic.Exporter
{
	public interface ILogFileExporter
	{
		void Export(IProgress<Percentage> progressReporter = null);
	}
}