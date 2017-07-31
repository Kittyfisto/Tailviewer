using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace Tailviewer.Core.Plugins
{
	/// <summary>
	///     Describes a .NET assembly contained in a plugin package.
	/// </summary>
	[DataContract]
	public sealed class AssemblyDescription : IAssemblyDescription
	{
		/// <inheritdoc />
		[DataMember]
		public string Fname { get; set; }

		/// <inheritdoc />
		public string AssemblyName
		{
			get
			{
				var name = Path.GetFileNameWithoutExtension(Fname);
				return name;
			}
		}

		/// <inheritdoc />
		[DataMember]
		public Version AssemblyVersion { get; set; }

		/// <inheritdoc />
		[DataMember]
		public Version AssemblyFileVersion { get; set; }

		/// <inheritdoc />
		[DataMember]
		public string AssemblyInformationalVersion { get; set; }

		/// <inheritdoc />
		[DataMember]
		public List<AssemblyReference> Dependencies { get; set; }

		/// <inheritdoc />
		[DataMember]
		public List<string> ImplementedPluginInterfaces { get; set; }

		IReadOnlyList<IAssemblyReference> IAssemblyDescription.Dependencies => Dependencies;

		IReadOnlyList<string> IAssemblyDescription.ImplementedPluginInterfaces => ImplementedPluginInterfaces;

		public static AssemblyDescription FromRawData(byte[] rawAssembly)
		{
			var assembly = Assembly.Load(rawAssembly);
			return FromAssembly(assembly);
		}

		public static AssemblyDescription FromFile(string assemblyFile)
		{
			var assembly = Assembly.LoadFrom(assemblyFile);
			return FromAssembly(assembly);
		}

		private static AssemblyDescription FromAssembly(Assembly assembly)
		{
			var description = new AssemblyDescription
			{
				//Fname = ,
				Dependencies = new List<AssemblyReference>(),
				ImplementedPluginInterfaces = new List<string>()
			};

			var assemblyVersion = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
			var assemblyFileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
			var assemblyInformationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

			if (assemblyVersion != null)
				description.AssemblyVersion = TryParse(assemblyVersion.Version);
			if (assemblyFileVersion != null)
				description.AssemblyFileVersion = TryParse(assemblyFileVersion.Version);
			if (assemblyInformationalVersion != null)
				description.AssemblyInformationalVersion = assemblyInformationalVersion.InformationalVersion;

			var references = assembly.GetReferencedAssemblies();
			foreach (var referencedAssembly in references)
				description.Dependencies.Add(new AssemblyReference
				{
					FullName = referencedAssembly.FullName
				});

			var pluginImplementations = assembly.GetTypes();

			return description;
		}

		[Pure]
		private static Version TryParse(string versionString)
		{
			Version version;
			Version.TryParse(versionString, out version);
			return version;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return AssemblyName ?? string.Empty;
		}
	}
}