using System;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Exporter
{
	public interface ILogFileExporter
	{
		void Export(IProgress<Percentage> progressReporter = null);
	}
}