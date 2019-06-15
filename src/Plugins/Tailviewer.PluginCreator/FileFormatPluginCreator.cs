using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tailviewer.PluginCreator
{
	internal sealed class FileFormatPluginCreator
	{
		public IReadOnlyList<string> CreateSourceFiles(string folder)
		{
			var files = new List<string>();
			files.Add(CreatePlugin(folder));
			return files;
		}

		private string CreatePlugin(string folder)
		{
			var builder = new StringBuilder();

			const string typeName = "FileFormatPluginImplementation";

			builder.AppendLine("using System.Threading;");
			builder.AppendLine("using System.Collections.Generic;");
			builder.AppendLine("using Tailviewer.BusinessLogic.Plugins;");
			builder.AppendLine("using Tailviewer.BusinessLogic.LogFiles;");
			builder.AppendFormat("public sealed class {0} : IFileFormatPlugin", typeName);
			builder.AppendLine();
			builder.AppendLine("{");
			builder.AppendLine("\tpublic IReadOnlyList<string> SupportedExtensions { get { return null; } }");
			builder.AppendLine("\tpublic ILogFile Open(string fileName, ITaskScheduler taskScheduler) { return null; }");
			builder.AppendLine("}");

			var fileName = Path.Combine(folder, typeName + ".cs");
			File.WriteAllText(fileName, builder.ToString());
			return fileName;
		}
	}
}