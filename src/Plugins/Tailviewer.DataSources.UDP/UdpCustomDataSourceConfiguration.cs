using System;
using System.Xml;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.DataSources.UDP
{
	public sealed class UdpCustomDataSourceConfiguration
		: ICustomDataSourceConfiguration
	{
		public string Address;

		#region Implementation of ICloneable

		object ICloneable.Clone()
		{
			return Clone();
		}

		public UdpCustomDataSourceConfiguration Clone()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Implementation of ICustomDataSourceConfiguration

		public void Restore(XmlReader reader)
		{
			for(int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "address":
						Address = reader.Value;
						break;
				}
			}
		}

		public void Store(XmlWriter writer)
		{
			writer.WriteAttributeString("address", Address);
		}

		#endregion
	}
}