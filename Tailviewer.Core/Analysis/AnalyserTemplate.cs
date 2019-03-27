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
		private LogAnalyserFactoryId _logAnalyserPluginId;
		private DataSourceAnalyserPluginId _dataSourceAnalyserPluginId;
		private AnalyserId _id;

		/// <inheritdoc />
		public LogAnalyserFactoryId LogAnalyserPluginId
		{
			get { return _logAnalyserPluginId; }
			set { _logAnalyserPluginId = value; }
		}

		/// <inheritdoc />
		public DataSourceAnalyserPluginId DataSourceAnalyserPluginId
		{
			get { return _dataSourceAnalyserPluginId; }
			set { _dataSourceAnalyserPluginId = value; }
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
			writer.WriteAttribute("LogAnalyserPluginId", _logAnalyserPluginId);
			writer.WriteAttribute("DataSourceAnalyserPluginId", _dataSourceAnalyserPluginId);
			writer.WriteAttribute("Id", _id);
			writer.WriteAttribute("Configuration", _configuration);
		}

		/// <inheritdoc />
		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("Id", out _id);

			if (!reader.TryReadAttribute("LogAnalyserPluginId", out _logAnalyserPluginId))
				reader.TryReadAttribute("FactoryId", out _logAnalyserPluginId); //< legacy name..

			reader.TryReadAttribute("DataSourceAnalyserPluginId", out _dataSourceAnalyserPluginId);
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
				LogAnalyserPluginId = LogAnalyserPluginId,
				DataSourceAnalyserPluginId = DataSourceAnalyserPluginId,
				Configuration = _configuration?.Clone() as ILogAnalyserConfiguration
			};
		}
	}
}