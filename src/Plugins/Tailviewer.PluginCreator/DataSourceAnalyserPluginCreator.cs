using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tailviewer.PluginCreator
{
	internal sealed class DataSourceAnalyserPluginCreator
	{
		public IEnumerable<string> CreateSourceFiles(string folder)
		{
			var files = new List<string>();
			files.Add(CreateDataSourceAnalyserPluginImplementation(folder));
			return files;
		}

		private string CreateDataSourceAnalyserPluginImplementation(string folder)
		{
			var builder = new StringBuilder();

			const string typeName = "DataSourceAnalyserPluginImplementation";

			builder.AppendLine("using System;");
			builder.AppendLine("using System.Threading;");
			builder.AppendLine("using Tailviewer;");
			builder.AppendLine("using Tailviewer.BusinessLogic;");
			builder.AppendLine("using Tailviewer.BusinessLogic.LogFiles;");
			builder.AppendLine("using Tailviewer.BusinessLogic.Analysis;");
			builder.AppendFormat("public sealed class {0} : IDataSourceAnalyserPlugin", typeName);
			builder.AppendLine();
			builder.AppendLine("{");
			builder.AppendLine("\tpublic AnalyserPluginId Id => new AnalyserPluginId();");
			builder.AppendLine("\tpublic IDataSourceAnalyser Create(AnalyserId id, ITaskScheduler scheduler, ILogFile logFile, ILogAnalyserConfiguration configuration) { throw new NotImplementedException(); }");
			builder.AppendLine("}");

			var fileName = Path.Combine(folder, typeName + ".cs");
			File.WriteAllText(fileName, builder.ToString());
			return fileName;
		}
	}
}