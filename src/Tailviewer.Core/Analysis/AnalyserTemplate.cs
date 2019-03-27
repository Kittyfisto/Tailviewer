using System;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     The template for a log analyser.
	///     Defines the id of the analyser instance, the factory which created the analyser and
	///     the analyser's configuration.
	/// </summary>
	public sealed class AnalyserTemplate
		: IAnalyserTemplate
	{
		private ILogAnalyserConfiguration _configuration;
		private AnalyserPluginId _analyserPluginId;
		private AnalyserId _id;

		/// <inheritdoc />
		public AnalyserPluginId AnalyserPluginId
		{
			get { return _analyserPluginId; }
			set { _analyserPluginId = value; }
		}

		/// <inheritdoc />
		public AnalyserId Id
		{
			get { return _id; }
			set { _id = value; }
		}

		/// <inheritdoc />
		public ILogAnalyserConfiguration Configuration
		{
			get { return _configuration; }
			set { _configuration = value; }
		}


		/// <inheritdoc />
		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("Id", _id);
			writer.WriteAttribute("AnalyserPluginId", _analyserPluginId);
			writer.WriteAttribute("Configuration", _configuration);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Id", out _id);

			if (!reader.TryReadAttribute("AnalyserPluginId", out _analyserPluginId))
				reader.TryReadAttribute("FactoryId", out _analyserPluginId); //< legacy name..

			reader.TryReadAttribute("Configuration", out _configuration);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		///     Returns a new clone of this template.
		/// </summary>
		/// <returns></returns>
		public AnalyserTemplate Clone()
		{
			return new AnalyserTemplate
			{
				Id = Id,
				AnalyserPluginId = AnalyserPluginId,
				Configuration = _configuration?.Clone() as ILogAnalyserConfiguration
			};
		}
	}
}