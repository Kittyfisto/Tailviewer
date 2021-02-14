using System.Collections.Generic;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.IO
{
	/// <summary>
	/// 
	/// </summary>
	public static class TextFileReaderExtensions
	{
		/// <summary>
		///    Reads the given contiguous section of the file.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="section"></param>
		/// <returns></returns>
		public static IReadOnlyList<string> Read(this ITextFileReader that, LogFileSection section)
		{
			var buffer = new string[section.Count];
			var linesRead = that.Read(section, buffer, 0);
			if (linesRead != buffer.Length)
			{
				var tmp = new string[linesRead];
				buffer.CopyTo(tmp, 0);
				return tmp;
			}
			return buffer;
		}

		/// <summary>
		///    Reads the non-contiguous given section of the file.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="section"></param>
		/// <returns></returns>
		public static IReadOnlyList<string> Read(this ITextFileReader that, IReadOnlyList<LogLineIndex> section)
		{
			var buffer = new string[section.Count];
			var linesRead = that.Read(section, buffer, 0);
			if (linesRead != buffer.Length)
			{
				var tmp = new string[linesRead];
				buffer.CopyTo(tmp, 0);
				return tmp;
			}
			return buffer;
		}
	}
}