using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;

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

		#region Implementation of IServiceContainer

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
			service = (T) factory.Retrieve();

			if (Log.IsDebugEnabled)
				Log.DebugFormat("Retrieved service {0}: {1}", interfaceType, factory.ImplementationType);

			return true;
		}

		#endregion

		/// <summary>
		///    Registers an object as an implementation of a particular service interface <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="service"></param>
		public void RegisterInstance<T>(T service)
			where T : class
		{
			if (service == null)
				throw new ArgumentNullException(nameof(service));

			var interfaceType = typeof(T);
			IServiceFactory existingFactory;

			lock (_syncRoot)
			{
				if (_servicesByInterface.TryGetValue(interfaceType, out existingFactory))
				{
					_servicesByInterface[interfaceType] = new Singleton<T>(service);
				}
				else
				{
					_servicesByInterface.Add(interfaceType, new Singleton<T>(service));
				}
			}

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

		private interface IServiceFactory
		{
			Type ImplementationType { get; }

			object Retrieve();
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

			public object Retrieve()
			{
				return _instance;
			}

			#endregion
		}
	}
}