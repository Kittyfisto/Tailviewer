using System;
using System.Reflection;

namespace Tailviewer.Ui.About
{
	public sealed class Dependency
	{
		private readonly string _licensePath;
		private readonly string _name;
		private readonly Version _version;
		private readonly Uri _website;

		public Dependency(string name, Version version, string website, string licensePath)
		{
			_name = name;
			_version = version;
			_licensePath = licensePath;
			_website = new Uri(website, UriKind.Absolute);
		}

		public Uri Website => _website;

		public string LicensePath => _licensePath;

		public string Name => _name;

		public Version Version => _version;

		public static Dependency CreateFrom<T>(string website, string licensePath)
		{
			Assembly assembly = typeof(T).Assembly;
			AssemblyName assemblyName = assembly.GetName();
			return new Dependency(assemblyName.Name, assemblyName.Version, website, licensePath);
		}
	}
}