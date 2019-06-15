using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tailviewer.PluginCreator
{
	internal sealed class LogAnalyserCreator
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

			const string typeName = "LogAnalyserImplementation";

			builder.AppendLine("using System;");
			builder.AppendLine("using System.Collections.Generic;");
			builder.AppendLine("using System.Threading;");
			builder.AppendLine("using Tailviewer;");
			builder.AppendLine("using Tailviewer.BusinessLogic;");
			builder.AppendLine("using Tailviewer.BusinessLogic.LogFiles;");
			builder.AppendLine("using Tailviewer.BusinessLogic.Analysis;");
			builder.AppendFormat("public sealed class {0} : ILogAnalyser", typeName);
			builder.AppendLine();
			builder.AppendLine("{");
			builder.AppendLine("\tpublic IReadOnlyList<Exception> UnexpectedExceptions => null;");
			builder.AppendLine("\tpublic TimeSpan AnalysisTime => TimeSpan.Zero;");
			builder.AppendLine("\tpublic ILogAnalysisResult Result => null;");
			builder.AppendLine("\tpublic Percentage Progress => Percentage.Zero;");
			builder.AppendLine("\tpublic void OnLogFileModified(ILogFile logFile, LogFileSection section) {}");
			builder.AppendLine("\tpublic void Dispose() {}");
			builder.AppendLine("}");

			var fileName = Path.Combine(folder, typeName + ".cs");
			File.WriteAllText(fileName, builder.ToString());
			return fileName;
		}
	}
}