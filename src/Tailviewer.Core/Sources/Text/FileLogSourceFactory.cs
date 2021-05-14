using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	/// 
	/// </summary>
	public static class FileLogSourceFactory
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="services"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static ILogSource OpenRead(IServiceContainer services, string fileName)
		{
			return new FileLogSource(services, fileName);
		}
	}
}