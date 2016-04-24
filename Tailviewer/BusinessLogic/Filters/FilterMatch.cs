using System.Text.RegularExpressions;

namespace Tailviewer.BusinessLogic.Filters
{
	public struct FilterMatch
	{
		public int Index;
		public int Count;

		public FilterMatch(int index, int count)
		{
			Index = index;
			Count = count;
		}

		public FilterMatch(Match regexMatch)
		{
			Index = regexMatch.Index;
			Count = regexMatch.Length;
		}

		public override string ToString()
		{
			return string.Format("Index: {0}, Count: {1}", Index, Count);
		}
	}
}