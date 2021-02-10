using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.IO
{
	/// <summary>
	/// 
	/// </summary>
	public static class TextFileReaderExtensions
	{
		/// <summary>
		/// 
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
	}
}