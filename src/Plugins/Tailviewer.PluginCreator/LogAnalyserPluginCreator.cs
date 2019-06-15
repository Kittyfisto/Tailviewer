using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tailviewer.PluginCreator
{
	sealed class LogAnalyserPluginCreator
	{
		public IEnumerable<string> CreateSourceFiles(string folder)
		{
			var files = new List<string>();
			files.Add(CreateLogAnalyserPlugin(folder));
			return files;
		}

		private string CreateLogAnalyserPlugin(string folder)
		{
			var builder = new StringBuilder();

			const string typeName = "LogAnalyserPluginImplementation";

			builder.AppendLine("using System;");
			builder.AppendLine("using System.Threading;");
			builder.AppendLine("using Tailviewer;");
			builder.AppendLine("using Tailviewer.BusinessLogic;");
			builder.AppendLine("using Tailviewer.BusinessLogic.LogFiles;");
			builder.AppendLine("using Tailviewer.BusinessLogic.Analysis;");
			builder.AppendFormat("public sealed class {0} : ILogAnalyserPlugin", typeName);
			builder.AppendLine();
			builder.AppendLine("{");
			builder.AppendLine("\tpublic AnalyserPluginId Id => new AnalyserPluginId();");
			builder.AppendLine("\tpublic ILogAnalyser Create(ITaskScheduler scheduler, ILogFile source, ILogAnalyserConfiguration configuration) { return null; }");
			builder.AppendLine("}");

			var fileName = Path.Combine(folder, typeName + ".cs");
			File.WriteAllText(fileName, builder.ToString());
			return fileName;
		}
	}
}
