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
		private readonly List<AnalyserTemplate> _analysers;

		/// <summary>
		///     Initializes this template.
		/// </summary>
		public AnalysisTemplate()
		{
			_analysers = new List<AnalyserTemplate>();
		}

		/// <summary>
		///     Initializes this template with the given analyser templates.
		/// </summary>
		public AnalysisTemplate(IEnumerable<AnalyserTemplate> analysers)
		{
			_analysers = new List<AnalyserTemplate>(analysers);
		}

		/// <inheritdoc />
		public IEnumerable<IAnalyserTemplate> Analysers => _analysers;

		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Analysers", _analysers);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Analysers", _analysers);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		///     Adds the given analyser to this template.
		/// </summary>
		/// <param name="template"></param>
		public void Add(AnalyserTemplate template)
		{
			_analysers.Add(template);
		}

		/// <summary>
		///     Removes the given analyser from this template.
		/// </summary>
		/// <param name="template"></param>
		public void Remove(AnalyserTemplate template)
		{
			_analysers.Remove(template);
		}


		/// <summary>
		///     Returns a deep clone of this template.
		/// </summary>
		/// <returns></returns>
		public AnalysisTemplate Clone()
		{
			return new AnalysisTemplate(_analysers.Select(x => x.Clone()));
		}
	}
}