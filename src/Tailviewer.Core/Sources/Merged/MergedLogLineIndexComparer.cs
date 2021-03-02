using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
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