using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.Settings;

namespace Tailviewer.Core
{
	/// <summary>
	/// </summary>
	public sealed class ServiceContainer
		: IServiceContainer
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly object _syncRoot;
		private readonly Dictionary<Type, IServiceFactory> _servicesByInterface;

		/// <summary>
		/// </summary>
		public ServiceContainer()
		{
			_syncRoot = new object();
			_servicesByInterface = new Dictionary<Type, IServiceFactory>();
		}

		private ServiceContainer(Dictionary<Type, IServiceFactory> servicesByInterface)
		{
			_syncRoot = new object();
			_servicesByInterface = servicesByInterface;
		}

		#region Implementation of IServiceContainer

		/// <inheritdoc />
		public IServiceContainer CreateChildContainer()
		{
			Dictionary<Type, IServiceFactory> services;
			lock (_syncRoot)
			{
				services = new Dictionary<Type, IServiceFactory>(_servicesByInterface.Count);
				foreach (var pair in _servicesByInterface)
				{
					services.Add(pair.Key, pair.Value);
				}
			}
			return new ServiceContainer(services);
		}

		/// <inheritdoc />
		public T Retrieve<T>() where T : class
		{
			var interfaceType = typeof(T);
			if (!TryRetrieve(out T service))
				throw new ArgumentException($"No service has been registered with this container which implements {interfaceType.FullName}");

			return service;
		}

		/// <inheritdoc />
		public bool TryRetrieve<T>(out T service) where T : class
		{
			var interfaceType = typeof(T);
			IServiceFactory factory;
			lock (_syncRoot)
			{
				if (!_servicesByInterface.TryGetValue(interfaceType, out factory))
				{
					service = null;
					return false;
				}
			}
			service = (T) factory.Retrieve(this);

			if (Log.IsDebugEnabled)
				Log.DebugFormat("Retrieved service {0}: {1}", interfaceType, factory.ImplementationType);

			return true;
		}

		/// <inheritdoc />
		public T TryRetrieve<T>() where T : class
		{
			if (!TryRetrieve(out T service))
				return null;

			return service;
		}

		/// <inheritdoc />
		public ILogFile CreateEventLogFile(string fileName)
		{
			return new EventLogFile(Retrieve<ITaskScheduler>(), fileName);
		}

		/// <inheritdoc />
		public ILogFile CreateFilteredLogFile(TimeSpan maximumWaitTime, ILogFile source, ILogEntryFilter filter)
		{
			return new FilteredLogFile(Retrieve<ITaskScheduler>(), maximumWaitTime, source,
			                           null,
			                           filter);
		}

		/// <inheritdoc />
		public ILogFileProxy CreateLogFileProxy(TimeSpan maximumWaitTime, ILogFile source)
		{
			return new LogFileProxy(Retrieve<ITaskScheduler>(), maximumWaitTime, source);
		}

		/// <inheritdoc />
		public IMergedLogFile CreateMergedLogFile(TimeSpan maximumWaitTime, IEnumerable<ILogFile> sources)
		{
			return new MergedLogFile(Retrieve<ITaskScheduler>(),
			                         maximumWaitTime,
			                         sources);
		}

		/// <inheritdoc />
		public ILogFile CreateMultiLineLogFile(TimeSpan maximumWaitTime, ILogFile source)
		{
			return new MultiLineLogFile(Retrieve<ITaskScheduler>(), source, maximumWaitTime);
		}

		/// <inheritdoc />
		public ILogFile CreateNoThrowLogFile(string pluginName, ILogFile source)
		{
			return new NoThrowLogFile(source, pluginName);
		}

		/// <inheritdoc />
		public ILogFile CreateTextLogFile(string fileName)
		{
			return new TextLogFile(this,
			                       fileName);
		}

		/// <inheritdoc />
		public void RegisterInstance<T>(T service)
			where T : class
		{
			if (service == null)
				throw new ArgumentNullException(nameof(service));

			var interfaceType = typeof(T);
			RegisterFactory(interfaceType, new Singleton<T>(service), out var existingFactory);

			if (Log.IsDebugEnabled)
			{
				if (existingFactory != null)
				{
					Log.DebugFormat("Replacing existing registration of service {0} (old implementation: {1}) with: {2}",
					                interfaceType,
					                existingFactory.ImplementationType,
					                service.GetType());
				}
				else
				{
					Log.DebugFormat("Registering service {0}: {1}",
					                interfaceType,
					                service.GetType());
				}
			}
		}

		#endregion

		/// <summary>
		///     Registers a factory which creates new objects whenever <see cref="Retrieve{T}" /> is called.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="factory"></param>
		public void RegisterType<T>(Func<IServiceContainer, T> factory) where T : class
		{
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			RegisterFactory(typeof(T), new Factory<T>(factory), out _);

			// TODO: Log
		}

		private void RegisterFactory(Type interfaceType, IServiceFactory newFactory, out IServiceFactory existingFactory)
		{
			lock (_syncRoot)
			{
				if (_servicesByInterface.TryGetValue(interfaceType, out existingFactory))
				{
					_servicesByInterface[interfaceType] = newFactory;
				}
				else
				{
					_servicesByInterface.Add(interfaceType, newFactory);
				}
			}
		}

		private interface IServiceFactory
		{
			Type ImplementationType { get; }

			object Retrieve(ServiceContainer container);
		}

		private sealed class Factory<T>
			: IServiceFactory
		{
			private readonly Func<IServiceContainer, T> _factory;

			public Factory(Func<IServiceContainer, T> factory)
			{
				_factory = factory;
			}

			#region Implementation of IServiceFactory

			public Type ImplementationType => null;

			public object Retrieve(ServiceContainer container)
			{
				return _factory(container);
			}

			#endregion
		}

		private sealed class Singleton<T>
			: IServiceFactory
		{
			private readonly T _instance;

			public Singleton(T instance)
			{
				_instance = instance;
			}

			#region Implementation of IServiceFactory

			public Type ImplementationType
			{
				get { return _instance.GetType(); }
			}

			public object Retrieve(ServiceContainer _)
			{
				return _instance;
			}

			#endregion
		}
	}
}