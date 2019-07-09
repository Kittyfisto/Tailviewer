using Tailviewer.Core;
using Tailviewer.Core.Settings;

namespace Tailviewer.BusinessLogic.Highlighters
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class Highlighter
	{
		/// <summary>
		///     The id of this highlighter.
		///     Is used to define for each data source which highlighter is active or not.
		/// </summary>
		public HighlighterId Id;

		/// <summary>
		///     How <see cref="Value" /> is to be intepreted.
		/// </summary>
		public FilterMatchType MatchType;

		/// <summary>
		///     The actual highlighter value, <see cref="MatchType" /> defines how it is interpreted.
		/// </summary>
		public string Value;

		/// <summary>
		///     Initializes this highlighter.
		/// </summary>
		public Highlighter()
		{
			Id = HighlighterId.CreateNew();
		}
	}
}
