using System;

namespace Tailviewer.BusinessLogic.LogTables.Parsers
{
	public sealed class ColumnHeader
		: IColumnHeader
	{
		private readonly string _name;

		public ColumnHeader(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			_name = name;
		}

		public string Name
		{
			get { return _name; }
		}

		public override string ToString()
		{
			return _name;
		}
	}
}