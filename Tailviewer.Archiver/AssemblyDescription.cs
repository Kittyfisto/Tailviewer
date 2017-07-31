using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
		public string EntryName { get; set; }

		/// <inheritdoc />
		[DataMember]
		public string AssemblyName { get; set; }

		/// <inheritdoc />
		public Version AssemblyVersion
		{
			get
			{
				Version version;
				Version.TryParse(SerializableAssemblyVersion, out version);
				return version;
			}
			set { SerializableAssemblyVersion = value != null ? value.ToString() : null; }
		}

		[DataMember]
		public string SerializableAssemblyVersion { get; set; }

		/// <inheritdoc />
		public Version AssemblyFileVersion
		{
			get
			{
				Version version;
				Version.TryParse(SerializableAssemblyFileVersion, out version);
				return version;
			}
			set { SerializableAssemblyFileVersion = value != null ? value.ToString() : null; }
		}

		[DataMember]
		public string SerializableAssemblyFileVersion { get; set; }

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

		public static AssemblyDescription FromAssembly(Assembly assembly)
		{
			var description = new AssemblyDescription
			{
				AssemblyName = assembly.GetName().Name,
				Dependencies = new List<AssemblyReference>(),
				ImplementedPluginInterfaces = new List<string>()
			};

			var assemblyFileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
			var assemblyInformationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

			description.AssemblyVersion = assembly.GetName().Version;
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