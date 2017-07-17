using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Plugins
{
	public sealed class PluginLoader
		: IPluginLoader
		, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public PluginLoader()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
		}

		private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			// We will allow the user to load assemblies that are not an exact match to the installed assemblies:
			// This list consists of the Api assembly and all of its dependencies:
			var assemblies = new Dictionary<string, Assembly>
			{
				{"log4net,", typeof(ILog).Assembly},
				{"Metrolib,", typeof(AbstractBootstrapper).Assembly},
				{"System.Threading.Extensions,", typeof(ITaskScheduler).Assembly},
				{"Tailviewer.Api,", typeof(ILogFile).Assembly}
			};

			// TODO: This feature most certainly needs proper tests. Go write them!

			foreach (var assembly in assemblies)
			{
				if (args.Name.StartsWith(assembly.Key))
				{
					Log.InfoFormat("Assembly '{0}' requests to load '{1}', resolving to '{2}'",
						args.RequestingAssembly,
						args.Name,
						assembly.Value
					);
					return assembly.Value;
				}
			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="description"></param>
		/// <returns></returns>
		public T Load<T>(IPluginDescription description) where T : class, IPlugin
		{
			if (description == null)
				throw new ArgumentNullException(nameof(description));

			var assembly = Assembly.LoadFrom(description.FilePath);
			var typeName = description.Plugins[typeof(T)];
			var implementation = assembly.GetType(typeName);
			if (implementation == null)
			{
				throw new ArgumentException(string.Format("Plugin '{0}' does not define a type named '{1}'",
					description.FilePath,
					typeName));
			}

			var plugin = (T) Activator.CreateInstance(implementation);
			return plugin;
		}

		public IEnumerable<T> LoadAllOfType<T>(IEnumerable<IPluginDescription> plugins)
			where T : class, IPlugin
		{
			var ret = new List<T>();
			foreach (var plugin in plugins)
			{
				if (plugin.Plugins.ContainsKey(typeof(T)))
				{
					ret.Add(Load<T>(plugin));
				}
			}
			return ret;
		}

		public void Dispose()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
		}
	}
}