using System;
using System.Xml;

namespace Tailviewer.Settings
{
	internal sealed class AutoUpdateSettings
	{
		public DateTime LastChecked;

		public void Save(XmlWriter writer)
		{
		}

		public void Restore(XmlReader reader)
		{
		}
	}
}