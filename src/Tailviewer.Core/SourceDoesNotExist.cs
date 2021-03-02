using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using Metrolib;
using Tailviewer.Api;

namespace Tailviewer.Core
{
	/// <summary>
	///     The data source (represented by the <see cref="ILogSource" />) doesn't exist.
	/// </summary>
	/// <remarks>
	///     Examples of when this flag should be set:
	///     - The data source represents a table from a SQL server and the connection was interrupted
	///     - The data source represents a file on disk and that has just been deleted
	/// </remarks>
	public sealed class SourceDoesNotExist
		: IEmptyReason
	{
		private readonly string _fullFileName;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fullFileName"></param>
		public SourceDoesNotExist(string fullFileName)
		{
			_fullFileName = fullFileName;
		}

		#region Implementation of IEmptyReason

		/// <inheritdoc />
		public string Reason => "Data source does not exist";

		/// <inheritdoc />
		public string Explanation => $"The data source '{Path.GetFileName(_fullFileName)}' was last seen {Path.GetDirectoryName(_fullFileName)}";

		/// <inheritdoc />
		public IReadOnlyList<string> Options => null;

		/// <inheritdoc />
		public Geometry Icon => Icons.FileRemove;

		#endregion
	}
}
