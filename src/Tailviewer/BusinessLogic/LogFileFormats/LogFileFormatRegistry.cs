using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Settings;
using Tailviewer.Settings.CustomFormats;

namespace Tailviewer.BusinessLogic.LogFileFormats
{
	internal sealed class LogFileFormatRegistry
		: ILogFileFormatRegistry
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IReadOnlyDictionary<PluginId, ICustomLogFileFormatCreatorPlugin> _creatorsById;
		private readonly Dictionary<CustomLogFileFormat, ILogFileFormat> _formatsByCustomFormat;
		private readonly List<ILogFileFormat> _formats;
		private readonly object _syncRoot;

		public LogFileFormatRegistry(IPluginLoader pluginLoader,
		                             ICustomFormatsSettings customFormats)
		{
			var creators = pluginLoader.LoadAllOfTypeWithDescription<ICustomLogFileFormatCreatorPlugin>();
			var creatorsById = new Dictionary<PluginId, ICustomLogFileFormatCreatorPlugin>(creators.Count);
			foreach(var description in creators)
			{
				if (description.Plugin != null)
				{
					creatorsById.Add(description.Description.Id, description.Plugin);
				}
			}

			_creatorsById = creatorsById;
			_formatsByCustomFormat = new Dictionary<CustomLogFileFormat, ILogFileFormat>();
			_syncRoot = new object();
			_formats = new List<ILogFileFormat>();

			foreach (var customFormat in customFormats)
			{
				TryAdd(customFormat);
			}
		}

		private void TryAdd(CustomLogFileFormat customFormat)
		{
			var id = customFormat.PluginId;
			if (id != null && _creatorsById.TryGetValue(id, out var creator))
			{
				try
				{
					if (creator.TryCreate(null, customFormat, out var format))
					{
						_formatsByCustomFormat.Add(customFormat, format);
						_formats.Add(format);
					}
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught exception while trying to create custom log file format '{0}' through plugin '{1}': {2}",
					                customFormat, creator, e);
				}
			}
		}

		#region Implementation of ILogFileFormatRegistry

		public IReadOnlyList<ILogFileFormat> Formats
		{
			get
			{
				lock (_syncRoot)
				{
					return _formats.ToList();
				}
			}
		}
		#endregion

		#region Implementation of ILogFileFormatRegistry

		public void Add(CustomLogFileFormat customFormat, ILogFileFormat format)
		{
			lock (_syncRoot)
			{
				_formats.Add(format);
			}
		}

		public void Remove(CustomLogFileFormat customFormat)
		{
			if (customFormat == null)
				return;

			lock (_syncRoot)
			{
				if (_formatsByCustomFormat.TryGetValue(customFormat, out var format))
				{
					_formatsByCustomFormat.Remove(customFormat);
					_formats.Remove(format);
				}
			}
		}

		public void Replace(CustomLogFileFormat oldCustomFormat, CustomLogFileFormat newCustomFormat, ILogFileFormat newFormat)
		{
			lock (_syncRoot)
			{
				if (oldCustomFormat != null && _formatsByCustomFormat.TryGetValue(oldCustomFormat, out var oldFormat))
				{
					_formatsByCustomFormat.Remove(oldCustomFormat);
					_formats.Remove(oldFormat);
				}

				if (newCustomFormat != null && newFormat != null)
				{
					_formatsByCustomFormat.Add(newCustomFormat, newFormat);
					_formats.Add(newFormat);
				}
			}
		}

		#endregion
	}
}
