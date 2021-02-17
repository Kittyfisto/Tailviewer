using System.Collections.Generic;

namespace Tailviewer.Core.Sources.Merged
{
	internal sealed class MergedLogLineIndexComparer
		: IComparer<MergedLogLineIndex>
	{
		#region Implementation of IComparer<in MergedLogLineIndex>

		public int Compare(MergedLogLineIndex x, MergedLogLineIndex y)
		{
			return x.Timestamp.CompareTo(y.Timestamp);
		}

		#endregion
	}
}