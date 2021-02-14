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
