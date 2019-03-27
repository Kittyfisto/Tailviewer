using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.Serialization;

namespace Tailviewer.Archiver.Plugins
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
		Version IAssemblyDescription.AssemblyVersion
		{
			get
			{
				Version version;
				Version.TryParse(AssemblyVersion, out version);
				return version;
			}
		}

		[DataMember]
		public string AssemblyVersion { get; set; }

		/// <inheritdoc />
		Version IAssemblyDescription.AssemblyFileVersion
		{
			get
			{
				Version version;
				Version.TryParse(AssemblyFileVersion, out version);
				return version;
			}
		}

		[DataMember]
		public string AssemblyFileVersion { get; set; }

		/// <inheritdoc />
		[DataMember]
		public string AssemblyInformationalVersion { get; set; }

		/// <inheritdoc />
		[DataMember]
		public List<AssemblyReference> Dependencies { get; set; }

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
			};

			var assemblyVersion = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
			var assemblyFileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
			var assemblyInformationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

			description.AssemblyVersion = assemblyVersion?.Version ?? assembly.GetName().Version?.ToString();
			if (assemblyFileVersion != null)
				description.AssemblyFileVersion = TryParse(assemblyFileVersion.Version)?.ToString();
			if (assemblyInformationalVersion != null)
				description.AssemblyInformationalVersion = assemblyInformationalVersion.InformationalVersion;

			var references = assembly.GetReferencedAssemblies();
			foreach (var referencedAssembly in references)
				description.Dependencies.Add(new AssemblyReference
				{
					FullName = referencedAssembly.FullName
				});

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