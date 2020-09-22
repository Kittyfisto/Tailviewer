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

		private readonly IReadOnlyDictionary<PluginId, ILogFileFormatCreatorPlugin> _creatorsById;
		private readonly List<ILogFileFormat> _formats;
		private readonly object _syncRoot;

		public LogFileFormatRegistry(IPluginLoader pluginLoader,
		                             ICustomFormatsSettings customFormats)
		{
			var creators = pluginLoader.LoadAllOfTypeWithDescription<ILogFileFormatCreatorPlugin>();
			var creatorsById = new Dictionary<PluginId, ILogFileFormatCreatorPlugin>(creators.Count);
			foreach(var description in creators)
			{
				if (description.Plugin != null)
				{
					creatorsById.Add(description.Description.Id, description.Plugin);
				}
			}

			_creatorsById = creatorsById;
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
					var format = creator.Create(customFormat);
					_formats.Add(format);
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

		public void Add(ILogFileFormat format)
		{
			lock (_syncRoot)
			{
				_formats.Add(format);
			}
		}

		public void Remove(ILogFileFormat format)
		{
			lock (_syncRoot)
			{
				_formats.Remove(format);
			}
		}

		public void Replace(ILogFileFormat old, ILogFileFormat @new)
		{
			lock (_syncRoot)
			{
				_formats.Add(old);
				_formats.Remove(@new);
			}
		}

		#endregion
	}
}
