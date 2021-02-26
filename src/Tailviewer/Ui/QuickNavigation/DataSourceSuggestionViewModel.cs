using System;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.QuickNavigation
{
	public sealed class DataSourceSuggestionViewModel
	{
		private readonly string _postfix;
		private readonly string _prefix;
		private readonly string _midfix;
		private readonly IDataSource _dataSource;

		public DataSourceSuggestionViewModel(IDataSource dataSource, string prefix, string midfix, string postfix)
		{
			if (dataSource == null)
				throw new ArgumentNullException(nameof(dataSource));

			_dataSource = dataSource;
			_prefix = prefix;
			_midfix = midfix;
			_postfix = postfix;
		}

		public string Prefix => _prefix;

		public string Midfix => _midfix;

		public string Postfix => _postfix;

		public IDataSource DataSource => _dataSource;

		public override string ToString()
		{
			return string.Format("{0}{1}{2}", _prefix, _midfix, _postfix);
		}
	}
}