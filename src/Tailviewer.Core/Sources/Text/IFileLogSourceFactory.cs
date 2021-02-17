using System.Text;

namespace Tailviewer.Core.Sources.Text
{
	/// <summary>
	/// 
	/// </summary>
	[Service]
	public interface IFileLogSourceFactory
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="format"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		ILogSource OpenRead(string fileName, ILogFileFormat format, Encoding encoding);
	}
}
