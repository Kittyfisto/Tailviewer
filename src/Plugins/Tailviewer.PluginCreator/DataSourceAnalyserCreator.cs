using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tailviewer.PluginCreator
{
	internal sealed class DataSourceAnalyserCreator
	{
		public IEnumerable<string> CreateSourceFiles(string folder)
		{
			var files = new List<string>();
			files.Add(CreateDataSourceAnalyserCreatorImplementation(folder));
			return files;
		}

		private string CreateDataSourceAnalyserCreatorImplementation(string folder)
		{
			var builder = new StringBuilder();

			const string typeName = "DataSourceAnalyserImplementation";

			builder.AppendLine("using System;");
			builder.AppendLine("using System.Threading;");
			builder.AppendLine("using Tailviewer;");
			builder.AppendLine("using Tailviewer.BusinessLogic;");
			builder.AppendLine("using Tailviewer.BusinessLogic.LogFiles;");
			builder.AppendLine("using Tailviewer.BusinessLogic.Analysis;");
			builder.AppendFormat("public sealed class {0} : IDataSourceAnalyser", typeName);
			builder.AppendLine();
			builder.AppendLine("{");
			builder.AppendLine("\tpublic AnalyserId Id => new AnalyserId();");
			builder.AppendLine("\tpublic AnalyserPluginId AnalyserPluginId => new AnalyserPluginId();");
			builder.AppendLine("\tpublic Percentage Progress => Percentage.Zero;");
			builder.AppendLine("\tpublic ILogAnalysisResult Result => null;");
			builder.AppendLine("\tpublic bool IsFrozen => false;");
			builder.AppendLine("\tpublic ILogAnalyserConfiguration Configuration { get {return null;} set {} }");
			builder.AppendLine("\tpublic void OnLogFileAdded(DataSourceId id, ILogFile logFile) {}");
			builder.AppendLine("\tpublic void OnLogFileRemoved(DataSourceId id, ILogFile logFile) {}");
			builder.AppendLine("\tpublic void Dispose() {}");
			builder.AppendLine("}");

			var fileName = Path.Combine(folder, typeName + ".cs");
			File.WriteAllText(fileName, builder.ToString());
			return fileName;
		}
	}
}