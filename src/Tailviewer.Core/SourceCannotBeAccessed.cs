using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using Metrolib;
using Tailviewer.Api;

namespace Tailviewer.Core
{
	/// <summary>
	///     The data source exists, but cannot be accessed.
	/// </summary>
	/// <remarks>
	///     Examples of when this flag should be set:
	///     - The user isn't authorized to view the file
	///     - The file is opened in exclusive mode by another application
	/// </remarks>
	public sealed class SourceCannotBeAccessed
		: IEmptyReason
	{
		private readonly string _fullFileName;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fullFileName"></param>
		public SourceCannotBeAccessed(string fullFileName)
		{
			_fullFileName = fullFileName;
		}

		#region Implementation of IEmptyReason

		/// <inheritdoc />
		public string Reason => "Data source cannot be opened";

		/// <inheritdoc />
		public string Explanation => $"The file '{Path.GetFileName(_fullFileName)}' may be opened exclusively by another process or you are not authorized to view it";

		/// <inheritdoc />
		public IReadOnlyList<string> Options => null;

		/// <inheritdoc />
		public Geometry Icon => Icons.FileAlert;

		#endregion
	}
}