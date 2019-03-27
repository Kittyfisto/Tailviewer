using System;
using System.IO;
using Tailviewer.Analysis.DataSources.BusinessLogic;

namespace Tailviewer.Analysis.DataSources.Ui
{
	public sealed class DataSourceViewModel
	{
		public string Name { get; }

		public DataSourceViewModel(DataSource dataSource)
		{
			try
			{
				Name = Path.GetFileName(dataSource.Name);
			}
			catch (Exception)
			{
				Name = dataSource.Name;
			}
		}
	}
}