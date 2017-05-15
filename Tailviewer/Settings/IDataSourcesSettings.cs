using System;
using System.Collections.Generic;

namespace Tailviewer.Settings
{
	public interface IDataSourcesSettings
		: IList<DataSource>
	{
		Guid SelectedItem { get; set; }

		/// <summary>
		///     Moves the element <paramref name="dataSource" /> to appear *anchor*
		///     <paramref name="anchor" />. Does nothing if this constraint doesn't
		///     exist of if either are not part of this list.
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="anchor"></param>
		void MoveBefore(DataSource dataSource, DataSource anchor);
	}
}