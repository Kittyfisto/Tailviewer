using System.Collections.Generic;

namespace Tailviewer.Settings.Analysis
{
	/// <summary>
	///     Represents view- and analysis configuration of an analysis.
	///     Can be used to start an active analysis.
	/// </summary>
	/// <remarks>
	///     Contains:
	///     - Pages and their layouts
	///     - Widgets per page
	///     - View configuration of every widget
	///     - Analysis configuration of every widget
	/// </remarks>
	public sealed class AnalysisTemplate
		: ISerializableType
	{
		private readonly List<AnalysisPageTemplate> _pages;

		public AnalysisTemplate()
		{
			_pages = new List<AnalysisPageTemplate>();
		}

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Pages", _pages);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Pages", _pages);
		}
	}
}
