using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Core;

namespace Tailviewer.Ui.SidePanel.Property
{
	public sealed class PropertyPresenterRegistry
		: IPropertyPresenterPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IReadOnlyList<IPropertyPresenterPlugin> _plugins;
		private readonly IReadOnlyDictionary<IReadOnlyPropertyDescriptor, string> _wellKnownDisplayNames;
		private readonly IReadOnlyDictionary<IReadOnlyPropertyDescriptor, Func<string, IPropertyPresenter>> _wellKnownPresenters;

		public PropertyPresenterRegistry(IPluginLoader pluginSystem)
		{
			_plugins = pluginSystem.LoadAllOfType<IPropertyPresenterPlugin>();

			_wellKnownDisplayNames = new Dictionary<IReadOnlyPropertyDescriptor, string>
			{
				{ Core.Properties.LogEntryCount, "Count" },
				{ Core.Properties.TraceLogEntryCount, "Trace Count" },
				{ Core.Properties.DebugLogEntryCount, "Debug Count" },
				{ Core.Properties.InfoLogEntryCount, "Info Count" },
				{ Core.Properties.WarningLogEntryCount, "Warning Count" },
				{ Core.Properties.ErrorLogEntryCount, "Error Count" },
				{ Core.Properties.FatalLogEntryCount, "Fatal Count" },
				{ Core.Properties.OtherLogEntryCount, "Other Count" },

				{ Core.Properties.Name, "Name" },
				{ Core.Properties.StartTimestamp, "First Timestamp" },
				{ Core.Properties.EndTimestamp, "Last Timestamp" },
				{ Core.Properties.Duration, "Duration" },
				{ Core.Properties.LastModified, "Last Modified" },
				{ Core.Properties.Created, "Created" },
				{ Core.Properties.Size, "Size" },

				{ Core.Properties.PercentageProcessed, "Processed" },
				{ Core.Properties.Format, "Format" },
				{ TextProperties.AutoDetectedEncoding, "Auto Detected Encoding" },
				{ TextProperties.OverwrittenEncoding, "Overwritten Encoding" },
				{ TextProperties.Encoding, "Encoding" }
			};
			_wellKnownPresenters = new Dictionary<IReadOnlyPropertyDescriptor, Func<string, IPropertyPresenter>>
			{
				{TextProperties.OverwrittenEncoding, displayName => new EncodingPropertyPresenter(displayName)}
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
				if (!_wellKnownDisplayNames.TryGetValue(property, out var displayName))
					return null; //< Well known properties without a display name are not intended to be shown...

				if (_wellKnownPresenters.TryGetValue(property, out var factory))
					return factory(displayName); //< For some properties, we offer specialized presenters

				return new DefaultPropertyPresenter(displayName); //< But for most, the default will do
			}

			// As far as other properties are concerned, we will just display them.
			return new DefaultPropertyPresenter(property.Id);
		}

		#endregion
	}
}
