using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogTables
{
	public interface ILogTableSchema
	{
		IReadOnlyCollection<IColumnHeader> ColumnHeaders { get; }
	}
}