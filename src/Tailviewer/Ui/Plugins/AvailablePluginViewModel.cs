using System;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.Archiver.Repository;

namespace Tailviewer.Ui.Plugins
{
	public sealed class AvailablePluginViewModel
		: IPluginViewModel
	{
		public AvailablePluginViewModel(PublishedPluginDescription plugin,
		                                Action download)
		{
			Name = plugin.Name;
			Description = plugin.Description;
			Author = plugin.Author;
			Version = plugin.Identifier.Version;

			var website = plugin.Website;
			Website = website != null ? new Uri(website) : null;
			DownloadCommand = new DelegateCommand2(download);
		}

		#region Implementation of IPluginViewModel

		public Version Version { get; }

		public string Author { get; }

		public string Name { get; }

		public string Description { get; }

		public string Error => null;

		public Uri Website { get; }

		public ImageSource Icon { get; }

		public ICommand DeleteCommand => null;

		public ICommand DownloadCommand { get; }

		public bool HasError => false;

		#endregion
	}
}
