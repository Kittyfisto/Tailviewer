using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Ui.Properties;

namespace Tailviewer.Ui.Controls.SidePanel.Outline
{
	public sealed class DefaultPropertyPresenterPlugin
		: IPropertyPresenterPlugin
	{
		private readonly IReadOnlyDictionary<IReadOnlyPropertyDescriptor, string> _displayNames;

		public DefaultPropertyPresenterPlugin()
		{
			_displayNames = new Dictionary<IReadOnlyPropertyDescriptor, string>
			{
				{ LogFileProperties.LogEntryCount, "Count" },
				{ LogFileProperties.Name, "Name" },
				{ LogFileProperties.StartTimestamp, "First Timestamp" },
				{ LogFileProperties.EndTimestamp, "Last Timestamp" },
				{ LogFileProperties.Duration, "Duration" },
				{ LogFileProperties.LastModified, "Last Modified" },
				{ LogFileProperties.Created, "Created" },
				{ LogFileProperties.Size, "Size" },

				{ LogFileProperties.PercentageProcessed, "Processed" },
				{ LogFileProperties.Format, "Format" },
				{ LogFileProperties.Encoding, "Encoding" }
			};
		}

		#region Implementation of IPropertyPresenterPlugin

		public IPropertyPresenter TryCreatePresenterFor(IReadOnlyPropertyDescriptor property)
		{
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
