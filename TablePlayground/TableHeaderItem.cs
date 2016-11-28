using Tailviewer.BusinessLogic.LogTables;

namespace TablePlayground
{
	public sealed class TableHeaderItem
	{
		private readonly IColumnHeader _header;

		public TableHeaderItem(IColumnHeader header)
		{
			_header = header;
		}

		public string Name
		{
			get { return _header.Name; }
		}
	}
}