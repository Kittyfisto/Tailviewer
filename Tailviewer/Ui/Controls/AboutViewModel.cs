using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Metrolib.Controls;
using log4net;

namespace Tailviewer.Ui.Controls
{
	public sealed class AboutViewModel
		: INotifyPropertyChanged
	{
		private readonly List<Dependency> _dependencies;
		private Dependency _selectedDependency;
		private string _selectedLicense;

		public AboutViewModel()
		{
			_dependencies = new List<Dependency>
				{
					Dependency.CreateFrom<ILog>("https://logging.apache.org/log4net/", "Licenses/Log4Net/LICENSE-2.0.txt"),
					Dependency.CreateFrom<FlatListView>("https://github.com/Kittyfisto/Metrolib", "Licenses/Metrolib/License.txt"),
					new Dependency("Inconsolata", new Version(1, 16), "https://fonts.google.com/specimen/Inconsolata", "Licenses/Inconsolata/OFL.txt")
				};
			SelectedDependency = _dependencies[0];
		}

		public IEnumerable<Dependency> Dependencies
		{
			get { return _dependencies; }
		}

		public Dependency SelectedDependency
		{
			get { return _selectedDependency; }
			set
			{
				if (value == _selectedDependency)
					return;

				_selectedDependency = value;
				SelectedLicense = value != null ? Resource.ReadResourceToEnd(value.LicensePath) : null;
				EmitPropertyChanged();
			}
		}

		public string SelectedLicense
		{
			get { return _selectedLicense; }
			set
			{
				if (value == _selectedLicense)
					return;
				_selectedLicense = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}

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

		public Uri Website
		{
			get { return _website; }
		}

		public string LicensePath
		{
			get { return _licensePath; }
		}

		public string Name
		{
			get { return _name; }
		}

		public Version Version
		{
			get { return _version; }
		}

		public static Dependency CreateFrom<T>(string website, string licensePath)
		{
			Assembly assembly = typeof (T).Assembly;
			AssemblyName assemblyName = assembly.GetName();
			return new Dependency(assemblyName.Name, assemblyName.Version, website, licensePath);
		}
	}
}