using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.IO
{
	/// <summary>
	/// 
	/// </summary>
	public interface ITextFileReader
		: IDisposable
	{
		/// <summary>
		///    
		/// </summary>
		void Start();

		/// <summary>
		///    Reads the given contiguous section of the file.
		/// </summary>
		/// <remarks>
		///    When the section requested is either partially or fully outside the valid range of the file, then this method will return an array of length of the
		///    exact number of bytes read from the file, ranging from 0 to the specified length (whichever was available).
		/// </remarks>
		/// <param name="section"></param>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		int Read(LogFileSection section, string[] buffer, int index);

		/// <summary>
		///    Reads the non-contiguous given section of the file.
		/// </summary>
		/// <remarks>
		///    When the section requested is either partially or fully outside the valid range of the file, then this method will return an array of length of the
		///    exact number of bytes read from the file, ranging from 0 to the specified length (whichever was available).
		/// </remarks>
		/// <param name="section"></param>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		int Read(IReadOnlyList<LogLineIndex> section, string[] buffer, int index);

		/// <summary>
		///    Asynchronously reads the given contiguous section of the file.
		/// </summary>
		/// <remarks>
		///    Returns immediately with a very low latency which is not tied to disk I/O.
		/// </remarks>
		/// <param name="section"></param>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		Task<int> ReadAsync(LogFileSection section, string[] buffer, int index);

		/// <summary>
		///    Asynchronously reads the given non-contiguous section of the file.
		/// </summary>
		/// <remarks>
		///    Returns immediately with a very low latency which is not tied to disk I/O.
		/// </remarks>
		/// <param name="section"></param>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		Task<int> ReadAsync(IReadOnlyList<LogLineIndex> section, string[] buffer, int index);
	}
}
