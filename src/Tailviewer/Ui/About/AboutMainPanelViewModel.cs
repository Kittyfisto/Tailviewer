using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using Metrolib.Controls;
using MMQ;
using Ookii.Dialogs.Wpf;
using Tailviewer.Settings;
using Tailviewer.Ui.MainPanel;
using Tailviewer.Ui.SidePanel;
using Xceed.Wpf.Toolkit;

namespace Tailviewer.Ui.About
{
	public sealed class AboutMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		private readonly List<Dependency> _dependencies;
		private Dependency _selectedDependency;
		private string _selectedLicense;
		
		public AboutMainPanelViewModel(IApplicationSettings applicationSettings) : base(applicationSettings)
		{
			_dependencies = new List<Dependency>
			{
				Dependency.CreateFrom<ILog>("https://logging.apache.org/log4net/", "Licenses/Log4Net/LICENSE-2.0.txt"),
				Dependency.CreateFrom<FlatListView>("https://github.com/Kittyfisto/Metrolib", "Licenses/Metrolib/License.txt"),
				Dependency.CreateFrom<IMemoryMappedQueue>("https://github.com/Kittyfisto/MMQ", "Licenses/MMQ/LICENSE"),
				Dependency.CreateFrom<ITask>("https://github.com/Kittyfisto/System.Threading.Extensions", "Licenses/System.Threading.Extensions/LICENSE"),
				new Dependency("Inconsolata", new Version(1, 16), "https://fonts.google.com/specimen/Inconsolata",
					"Licenses/Inconsolata/OFL.txt"),
				new Dependency("Google Material Design Icons", new Version(3, 0, 1, 0),
					"https://github.com/google/material-design-icons", "Licenses/GoogleMaterialDesignIcons/License.txt"),
				new Dependency("Material Design Icons", new Version(1, 7, 22, 0),
					"https://github.com/Templarian/MaterialDesign", "Licenses/MaterialDesignIcons/License.txt"),
				Dependency.CreateFrom<VistaFolderBrowserDialog>("https://github.com/caioproiete/ookii-dialogs-wpf", "Licenses/Ookii.Dialogs.Wpf/LICENSE"),
				Dependency.CreateFrom<ColorPicker>("https://github.com/xceedsoftware/wpftoolkit", "Licenses/Xceed.Wpf.Toolkit/LICENSE")
			};
			SelectedDependency = _dependencies[0];
		}

		public IEnumerable<Dependency> Dependencies => _dependencies;

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

		public override IEnumerable<ISidePanelViewModel> SidePanels => Enumerable.Empty<ISidePanelViewModel>();

		public override void Update()
		{ }
	}
}
