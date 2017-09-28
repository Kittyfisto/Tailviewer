using System;
using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.Core.Analysis
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
		: IAnalysisTemplate
	{
		private readonly List<AnalysisPageTemplate> _pages;

		/// <summary>
		///     Initializes this template.
		/// </summary>
		public AnalysisTemplate()
		{
			_pages = new List<AnalysisPageTemplate>();
		}

		private AnalysisTemplate(IEnumerable<AnalysisPageTemplate> pages)
		{
			_pages = new List<AnalysisPageTemplate>(pages);
		}

		/// <inheritdoc />
		public IEnumerable<IAnalysisPageTemplate> Pages => _pages;

		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Pages", _pages);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Pages", _pages);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		///     Adds the given page to this template.
		/// </summary>
		/// <param name="template"></param>
		public void Add(AnalysisPageTemplate template)
		{
			_pages.Add(template);
		}

		/// <summary>
		///     Returns a deep clone of this template.
		/// </summary>
		/// <returns></returns>
		public AnalysisTemplate Clone()
		{
			return new AnalysisTemplate(_pages.Select(x => x.Clone()));
		}
	}
}