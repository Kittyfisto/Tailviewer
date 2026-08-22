using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Core;

using Tailviewer.Localization;

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
				{ Core.Properties.LogEntryCount, Strings.Count },
				{ Core.Properties.TraceLogEntryCount, Strings.TraceCount },
				{ Core.Properties.DebugLogEntryCount, Strings.DebugCount },
				{ Core.Properties.InfoLogEntryCount, Strings.InfoCount },
				{ Core.Properties.WarningLogEntryCount, Strings.WarningCount },
				{ Core.Properties.ErrorLogEntryCount, Strings.ErrorCount },
				{ Core.Properties.FatalLogEntryCount, Strings.FatalCount },
				{ Core.Properties.OtherLogEntryCount, Strings.OtherCount },

				{ Core.Properties.Name, Strings.PropertyName },
				{ Core.Properties.StartTimestamp, Strings.FirstTimestamp },
				{ Core.Properties.EndTimestamp, Strings.LastTimestamp },
				{ Core.Properties.Duration, Strings.Duration },
				{ Core.Properties.LastModified, Strings.LastModified },
				{ Core.Properties.Created, Strings.Created },
				{ Core.Properties.Size, Strings.Size },

				{ Core.Properties.PercentageProcessed, Strings.Processed },
				{ Core.Properties.Format, Strings.Format },
				{ TextProperties.AutoDetectedEncoding, Strings.AutoDetectedEncoding },
				{ TextProperties.OverwrittenEncoding, Strings.OverwrittenEncoding },
				{ TextProperties.Encoding, Strings.Encoding }
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
