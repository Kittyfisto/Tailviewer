using System.Runtime.Serialization;

namespace Tailviewer.Core.Plugins
{
	/// <summary>
	///     Describes an assembly referenced by another assembly in a plugin package.
	/// </summary>
	[DataContract]
	public sealed class AssemblyReference : IAssemblyReference
	{
		/// <inheritdoc />
		[DataMember]
		public string FullName { get; set; }

		/// <inheritdoc />
		public override string ToString()
		{
			return FullName ?? string.Empty;
		}
	}
}