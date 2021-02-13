using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tailviewer.PluginCreator
{
	internal sealed class LogFileCreator
	{
		public IEnumerable<string> CreateSourceFiles(string folder)
		{
			var files = new List<string>();
			files.Add(CreateLogFileImplementation(folder));
			return files;
		}

		private string CreateLogFileImplementation(string folder)
		{
			var builder = new StringBuilder();

			var typeName = "LogFileImplementation";

			builder.AppendLine("using System;");
			builder.AppendLine("using System.Collections.Generic;");
			builder.AppendLine("using Tailviewer.BusinessLogic;");
			builder.AppendLine("using Tailviewer.BusinessLogic.LogFiles;");
			builder.AppendFormat("public sealed class {0} : ILogFile", typeName);
			builder.AppendLine();
			builder.AppendLine("{");
			builder.AppendLine("\tpublic void Dispose() {}");
			builder.AppendLine("\tpublic bool EndOfSourceReached => false;");
			builder.AppendLine("\tpublic int Count => 0;");
			builder.AppendLine("\tpublic int OriginalCount => 0;");
			builder.AppendLine("\tpublic int MaxCharactersPerLine => 0;");
			builder.AppendLine("\tpublic IReadOnlyList<ILogFileColumn> Columns => null;");
			builder.AppendLine("\tpublic void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount) {}");
			builder.AppendLine("\tpublic void RemoveListener(ILogFileListener listener) {}");
			builder.AppendLine("\tpublic IReadOnlyList<ILogFilePropertyDescriptor> Properties => null;");
			builder.AppendLine("\tpublic object GetValue(ILogFilePropertyDescriptor propertyDescriptor) {return null;}");
			builder.AppendLine("\tpublic T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor) { throw new NotImplementedException();}");
			builder.AppendLine("\tpublic void GetValues(ILogFileProperties properties) {}");
			builder.AppendLine("\tpublic void GetColumn<T>(LogFileSection section, ILogFileColumnDescriptor<T> column, T[] buffer, int destinationIndex) {}");
			builder.AppendLine("\tpublic void GetColumn<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumnDescriptor<T> column, T[] buffer, int destinationIndex) {}");
			builder.AppendLine("\tpublic void GetEntries(LogFileSection section, ILogEntries buffer, int destinationIndex) {}");
			builder.AppendLine("\tpublic void GetEntries(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex) {}");
			builder.AppendLine("\tpublic double Progress => 0;");
			builder.AppendLine("\tpublic LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex) {return new LogLineIndex();}");
			builder.AppendLine("}");

			var fileName = Path.Combine(folder, typeName + ".cs");
			File.WriteAllText(fileName, builder.ToString());
			return fileName;
		}
	}
}