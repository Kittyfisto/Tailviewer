using System.Collections.Generic;

namespace Tailviewer.Core.LogTables
{
	/// <summary>
	/// 
	/// </summary>
	public interface ILogTableSchema
	{
		/// <summary>
		/// 
		/// </summary>
		IReadOnlyCollection<IColumnHeader> ColumnHeaders { get; }
	}
}