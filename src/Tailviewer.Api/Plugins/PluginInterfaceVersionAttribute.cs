using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Api
{
	/// <summary>
	///     This attribute is used by tailviewer to version <see cref="IPlugin" /> interfaces.
	///     Every <see cref="IPlugin" /> interface has version 0, unless attributed differently.
	///     A particular plugin can only be used if it implements the same version tailviewer supports.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface)]
	public sealed class PluginInterfaceVersionAttribute
		: Attribute
	{
		/// <summary>
		/// </summary>
		/// <param name="version"></param>
		public PluginInterfaceVersionAttribute(int version)
		{
			Version = new PluginInterfaceVersion(version);
		}

		/// <summary>
		///     The version number of this interface.
		///     If a plugin implements an interface of either a higher or lower number than what tailviewer
		///     currently supports, then that specific plugin cannot be used.
		/// </summary>
		public PluginInterfaceVersion Version { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static PluginInterfaceVersion GetInterfaceVersion(Type type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			var interfaces = type.GetInterfaces().ToList();
			if (type.IsInterface)
				interfaces.Add(type);

			foreach (var @interface in interfaces)
			{
				if (ImplementsIPlugin(@interface))
				{
					var attribute = @interface.GetCustomAttribute<PluginInterfaceVersionAttribute>();
					if (attribute == null)
					{
						return PluginInterfaceVersion.First;
					}

					return attribute.Version;
				}
			}

			throw new ArgumentException($"The type '{type.FullName}' does not implement any IPlugin interface");
		}

		[Pure]
		private static bool ImplementsIPlugin(Type @interface)
		{
			var interfaces = @interface.GetInterfaces();
			if (interfaces.Contains(typeof(IPlugin)))
				return true;

			return false;
		}
	}
}