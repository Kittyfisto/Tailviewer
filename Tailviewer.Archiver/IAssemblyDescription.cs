using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tailviewer.Core.Plugins
{
	/// <summary>
	///     Describes an assembly which is part of
	/// </summary>
	public interface IAssemblyDescription
	{
		/// <summary>
		///     The file name of the assembly in the package.
		/// </summary>
		string EntryName { get; }

		/// <summary>
		///     The name of the assembly (i.e. excluding path and extension).
		/// </summary>
		string AssemblyName { get; }

		/// <summary>
		///     The version of the assembly, as mentioned in its <see cref="AssemblyVersionAttribute" />.
		/// </summary>
		Version AssemblyVersion { get; }

		/// <summary>
		///     The file version of the assembly, as mentioned in its <see cref="AssemblyFileVersionAttribute" />.
		/// </summary>
		Version AssemblyFileVersion { get; }

		/// <summary>
		///     The informational version of the assembly, as mentioned in its <see cref="AssemblyInformationalVersionAttribute" />
		///     .
		/// </summary>
		string AssemblyInformationalVersion { get; }

		/// <summary>
		///     The referenced assemblies this assembly dependens on.
		/// </summary>
		IReadOnlyList<IAssemblyReference> Dependencies { get; }
	}
}