using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.BusinessLogic.Highlighters
{
	/// <summary>
	///     Responsible for maintaining the list of available highlighters a user has created.
	/// </summary>
	/// <remarks>
	///     Eventually, highlighters will be persisted between sessions.
	/// </remarks>
	public sealed class HighlighterCollection
		: IHighlighters
	{
		private readonly List<Highlighter> _highlighters;
		private readonly object _syncRoot;

		public HighlighterCollection()
		{
			_highlighters = new List<Highlighter>();
			_syncRoot = new object();
		}

		#region Implementation of IHighlighters

		public IEnumerable<Highlighter> Highlighters
		{
			get
			{
				lock (_syncRoot)
				{
					return _highlighters.ToList();
				}
			}
		}

		public Highlighter AddHighlighter()
		{
			var highlighter = new Highlighter();

			lock (_syncRoot)
			{
				_highlighters.Add(highlighter);
			}

			return highlighter;
		}

		public void Remove(HighlighterId id)
		{
			lock (_syncRoot)
			{
				_highlighters.RemoveAll(x => x.Id == id);
			}
		}

		#endregion
	}
}