using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Ui.Properties;

namespace Tailviewer.Ui.Controls.SidePanel.Outline
{
	public sealed class PropertyPresenterRegistry
		: IPropertyPresenterPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IReadOnlyList<IPropertyPresenterPlugin> _plugins;
		private readonly IReadOnlyDictionary<IReadOnlyPropertyDescriptor, string> _displayNames;

		public PropertyPresenterRegistry(IPluginLoader pluginSystem)
		{
			_plugins = pluginSystem.LoadAllOfType<IPropertyPresenterPlugin>();

			_displayNames = new Dictionary<IReadOnlyPropertyDescriptor, string>
			{
				{ Core.LogFiles.Properties.LogEntryCount, "Count" },
				{ Core.LogFiles.Properties.Name, "Name" },
				{ Core.LogFiles.Properties.StartTimestamp, "First Timestamp" },
				{ Core.LogFiles.Properties.EndTimestamp, "Last Timestamp" },
				{ Core.LogFiles.Properties.Duration, "Duration" },
				{ Core.LogFiles.Properties.LastModified, "Last Modified" },
				{ Core.LogFiles.Properties.Created, "Created" },
				{ Core.LogFiles.Properties.Size, "Size" },

				{ Core.LogFiles.Properties.PercentageProcessed, "Processed" },
				{ Core.LogFiles.Properties.Format, "Format" },
				{ Core.LogFiles.Properties.Encoding, "Encoding" }
			};
		}

		#region Implementation of IPropertyPresenterPlugin

		public IPropertyPresenter TryCreatePresenterFor(IReadOnlyPropertyDescriptor property)
		{
			foreach (var plugin in _plugins)
			{
				try
				{
					var presenter = plugin.TryCreatePresenterFor(property);
					if (presenter != null)
						return presenter;
				}
				catch (Exception e)
				{
					Log.WarnFormat("Caught unexpected exception: {0}", e);
				}
			}

			if (property is IWellKnownReadOnlyPropertyDescriptor wellKnownProperty)
			{
				if (!_displayNames.TryGetValue(property, out var displayName))
					return null;

				return new DefaultPropertyPresenter(displayName);
			}

			return new DefaultPropertyPresenter(property.Id);
		}

		#endregion
	}
}
