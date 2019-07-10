﻿using System.Runtime.Serialization;

namespace Tailviewer.Archiver.Repository
{
	[DataContract]
	public sealed class PluginInterface
	{
		public PluginInterface()
		{ }

		public PluginInterface(string fullName, int version)
		{
			FullName = fullName;
			Version = version;
		}

		[DataMember]
		public string FullName { get; set; }

		[DataMember]
		public int Version { get; set; }

		#region Overrides of ValueType

		public override string ToString()
		{
			return $"{FullName} v{Version}";
		}

		#endregion
	}
}