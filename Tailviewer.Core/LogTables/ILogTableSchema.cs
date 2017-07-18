using System.Collections.Generic;

namespace Tailviewer.Core.LogTables
{
	public interface ILogTableSchema
	{
		IReadOnlyCollection<IColumnHeader> ColumnHeaders { get; }
	}
}