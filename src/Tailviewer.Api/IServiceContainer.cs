using System;
using System.Collections.Generic;
using System.Text;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;

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
	///     - <see cref="Tailviewer.ITypeFactory" />
	///     - <see cref="System.Threading.ITaskScheduler" />
	///     - <see cref="System.Threading.ISerialTaskScheduler" />
	///     - <see cref="System.IO.IFilesystem" />
	///     - <see cref="ILogFileFormatRepository"/>
	///     - IPluginLoader
	/// </remarks>
	public interface IServiceContainer
	{
		/// <summary>
		///     Creates a new container which contains the same configuration
		///     as this container, but allows the caller to make changes without
		///     affecting this container.
		/// </summary>
		/// <returns></returns>
		IServiceContainer CreateChildContainer();

		/// <summary>
		///     Registers an object as an implementation of a particular service interface <typeparamref name="T" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="service"></param>
		void RegisterInstance<T>(T service) where T : class;

		/// <summary>
		///     Retrieves a service which implements the interface <typeparamref name="T" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <exception cref="ArgumentException">
		///     In case no service which implements the interface <typeparamref name="T" /> has
		///     been registered with this container.
		/// </exception>
		T Retrieve<T>() where T : class;

		/// <summary>
		///     Retrieves a service which implements the interface <typeparamref name="T" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="service"></param>
		/// <returns>
		///     True in case a service implementing <typeparamref name="T" /> has been registered and is returned, false
		///     otherwise
		/// </returns>
		bool TryRetrieve<T>(out T service) where T : class;

		/// <summary>
		///     Retrieves a service which implements the interface <typeparamref name="T" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>
		///     The service implementing <typeparamref name="T" /> in case it has been registered with this container,
		///     or null otherwise.
		/// </returns>
		T TryRetrieve<T>() where T : class;

		#region Tailviewer specific objects

		/// <summary>
		///     Creates a new log file object which interprets the given file as a windows event log (ETW) file.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		ILogFile CreateEventLogFile(string fileName);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.FilteredLogFile type.
		/// </summary>
		/// <param name="maximumWaitTime"></param>
		/// <param name="source"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		ILogFile CreateFilteredLogFile(TimeSpan maximumWaitTime,
		                               ILogFile source,
		                               ILogEntryFilter filter);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.LogFileProxy type.
		/// </summary>
		/// <param name="maximumWaitTime"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		ILogFileProxy CreateLogFileProxy(TimeSpan maximumWaitTime, ILogFile source);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.MergedLogFile type.
		/// </summary>
		/// <param name="maximumWaitTime"></param>
		/// <param name="sources"></param>
		/// <returns></returns>
		IMergedLogFile CreateMergedLogFile(TimeSpan maximumWaitTime, IEnumerable<ILogFile> sources);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.MultiLineLogFile type.
		/// </summary>
		/// <param name="maximumWaitTime"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		ILogFile CreateMultiLineLogFile(TimeSpan maximumWaitTime, ILogFile source);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.NoThrowLogFile type.
		/// </summary>
		/// <param name="pluginName"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		ILogFile CreateNoThrowLogFile(string pluginName, ILogFile source);

		/// <summary>
		///     Creates a new log file object which interprets the given file as a text file.
		/// </summary>
		/// <remarks>
		///     The following types may optionally be registered with this container in order to modify the behaviour of the returned object:
		///     - <see cref="ITimestampParser"/>
		///     - <see cref="ILogLineTranslator"/>
		///     - <see cref="Encoding"/>
		/// </remarks>
		/// <param name="fileName"></param>
		/// <returns></returns>
		ILogFile CreateTextLogFile(string fileName);

		#endregion
	}
}