﻿using System;
using System.Collections.Generic;

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
		/// <exception cref="NoSuchServiceException">
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
		ILogSource CreateEventLogFile(string fileName);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.FilteredLogFile type.
		/// </summary>
		/// <param name="maximumWaitTime"></param>
		/// <param name="source"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		ILogSource CreateFilteredLogFile(TimeSpan maximumWaitTime,
		                               ILogSource source,
		                               ILogEntryFilter filter);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.LogFileProxy type.
		/// </summary>
		/// <param name="maximumWaitTime"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		ILogSourceProxy CreateLogFileProxy(TimeSpan maximumWaitTime, ILogSource source);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.MergedLogFile type.
		/// </summary>
		/// <param name="maximumWaitTime"></param>
		/// <param name="sources"></param>
		/// <returns></returns>
		IMergedLogFile CreateMergedLogFile(TimeSpan maximumWaitTime, IEnumerable<ILogSource> sources);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.MultiLineLogFile type.
		/// </summary>
		/// <param name="maximumWaitTime"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		ILogSource CreateMultiLineLogFile(TimeSpan maximumWaitTime, ILogSource source);

		/// <summary>
		///     Creates a new instance of the Tailviewer.Core.LogFiles.NoThrowLogFile type.
		/// </summary>
		/// <param name="pluginName"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		ILogSource CreateNoThrowLogFile(string pluginName, ILogSource source);

		#endregion
	}
}