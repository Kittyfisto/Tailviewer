using System;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     Groups a <see cref="AnalysisTemplate" /> and <see cref="AnalysisViewTemplate" />.
	/// </summary>
	public sealed class ActiveAnalysisConfiguration
		: ISerializableType
			, ICloneable
	{
		private AnalysisId _id;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		public ActiveAnalysisConfiguration()
		{
			_id = AnalysisId.Empty;
			Template = new AnalysisTemplate();
			ViewTemplate = new AnalysisViewTemplate();
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		public ActiveAnalysisConfiguration(AnalysisId id, AnalysisTemplate template, AnalysisViewTemplate viewTemplate)
		{
			if (template == null)
				throw new ArgumentNullException(nameof(template));
			if (viewTemplate == null)
				throw new ArgumentNullException(nameof(viewTemplate));

			_id = id;
			Template = template;
			ViewTemplate = viewTemplate;
		}

		/// <summary>
		///     The id of this analysis - unique amongst every analysis that ever was and ever will be.
		/// </summary>
		public AnalysisId Id => _id;

		/// <summary>
		///     The template of this analysis.
		/// </summary>
		public AnalysisTemplate Template { get; }

		/// <summary>
		///     The view template of this analysis.
		/// </summary>
		public AnalysisViewTemplate ViewTemplate { get; }

		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("AnalysisId", _id);
			writer.WriteAttribute("Template", Template);
			writer.WriteAttribute("ViewTemplate", ViewTemplate);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("AnalysisId", out _id);
			reader.TryReadAttribute("Template", Template);
			reader.TryReadAttribute("ViewTemplate", ViewTemplate);
		}

		/// <summary>
		///     Returns a deep clone of this object.
		/// </summary>
		/// <returns></returns>
		public ActiveAnalysisConfiguration Clone()
		{
			throw new NotImplementedException();
		}
	}
}