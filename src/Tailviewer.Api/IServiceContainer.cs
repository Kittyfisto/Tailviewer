using System;

namespace Tailviewer
{
	/// <summary>
	///     Responsible for providing access to all kinds of services.
	/// </summary>
	/// <remarks>
	///     Services offered by this container are usually marked with the <see cref="ServiceAttribute" />,
	///     unless they are external dependencies.
	/// </remarks>
	/// <remarks>
	///     If you are a plugin author, then you can typically expect to retrieve the following list of service interfaces
	///     from this container at runtime:
	///     - <see cref="Tailviewer.ITypeFactory"/>
	///     - <see cref="System.Threading.ITaskScheduler"/>
	///     - <see cref="System.Threading.ISerialTaskScheduler"/>
	///     - <see cref="System.IO.IFilesystem"/>
	/// </remarks>
	public interface IServiceContainer
	{
		/// <summary>
		///     Retrieves a service which implements the interface <typeparamref name="T" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <exception cref="ArgumentException">In case no service which implements the interface <typeparamref name="T"/> has been registered with this container.</exception>
		T Retrieve<T>() where T : class;

		/// <summary>
		///     Retrieves a service which implements the interface <typeparamref name="T" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="service"></param>
		/// <returns>True in case a service implementing <typeparamref name="T"/> has been registered and is returned, false otherwise</returns>
		bool TryRetrieve<T>(out T service) where T : class;
	}
}