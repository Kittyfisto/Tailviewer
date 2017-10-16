using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.DataSources
{
	/// <summary>
	///     The interface for a data source which is constructed from one or more <see cref="IDataSource" />s.
	/// </summary>
	public interface IMergedDataSource
		: IDataSource
	{
		/// <summary>
		///     The data sources which make up this merged data source.
		/// </summary>
		/// <remarks>
		///     Data sources are ordered in a way to match <see cref="LogLine.SourceId" />:
		///     If a line has been provided by source 4, then that source is the fourth
		///     entry in this list.
		/// </remarks>
		IReadOnlyList<IDataSource> OriginalSources { get; }
	}
}