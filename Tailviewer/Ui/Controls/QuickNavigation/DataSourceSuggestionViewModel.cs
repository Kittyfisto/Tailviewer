using System;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.Controls.QuickNavigation
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

		private bool Equals(DataSourceSuggestionViewModel other)
		{
			return _dataSource.Equals(other._dataSource);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is DataSourceSuggestionViewModel && Equals((DataSourceSuggestionViewModel) obj);
		}

		public override int GetHashCode()
		{
			return _dataSource.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("{0}{1}{2}", _prefix, _midfix, _postfix);
		}
	}
}