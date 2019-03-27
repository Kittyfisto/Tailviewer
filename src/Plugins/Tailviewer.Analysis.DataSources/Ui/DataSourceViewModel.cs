using System;
using System.IO;
using Tailviewer.DataSources.BusinessLogic;

namespace Tailviewer.DataSources.Ui
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