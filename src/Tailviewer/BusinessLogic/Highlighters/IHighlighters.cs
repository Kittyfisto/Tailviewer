using System.Collections.Generic;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Highlighters
{
	public interface IHighlighters
	{
		IEnumerable<Highlighter> Highlighters { get; }

		/// <summary>
		///     Adds a new quickfilter.
		/// </summary>
		/// <returns></returns>
		Highlighter AddHighlighter();

		void Remove(HighlighterId id);
	}
}
