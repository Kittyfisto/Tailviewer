using System.Runtime.Serialization;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.PluginRepository.Entities
{
	/// <summary>
	///    Describes a singular change made to a plugin.
	/// </summary>
	/// <remarks>
	///     Making breaking changes to this type is not advised because it will
	///     break existing repositories.
	/// </remarks>
	[DataContract]
	public sealed class Change
	{
		public Change()
		{}

		public Change(SerializableChange change)
		{
			Summary = change.Summary;
			Description = change.Description;
		}

		/// <summary>
		///    A short (one sentence) summary of the change, mandatory.
		/// </summary>
		[DataMember]
		public string Summary { get; set; }

		/// <summary>
		///    An optional (detailed) description of the change.
		/// </summary>
		[DataMember]
		public string Description { get; set; }
	}
}