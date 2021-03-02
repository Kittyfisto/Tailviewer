using System;

namespace Tailviewer.Api
{
	/// <summary>
	///    This exception is thrown when a service which is not registered with the <see cref="IServiceContainer"/> is accessed.
	/// </summary>
	public sealed class NoSuchServiceException
		: ArgumentException
	{
		/// <summary>
		/// 
		/// </summary>
		public NoSuchServiceException(Type missingService)
			: base($"No service has been registered with this container which implements {missingService.FullName}")
		{}
	}
}