using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic
{
	internal sealed class DataSources
		: IEnumerable<DataSource>
		, IDisposable
	{
		private readonly DataSourcesSettings _settings;
		private readonly object _syncRoot;
		private readonly List<DataSource> _dataSources;

		public DataSources(DataSourcesSettings settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");

			_syncRoot = new object();
			_settings = settings;
			_dataSources = new List<DataSource>();
			foreach (var dataSource in settings)
			{
				Add(dataSource);
			}
		}

		public int Count
		{
			get
			{
				lock (_syncRoot)
				{
					return _dataSources.Count;
				}
			}
		}

		public DataSource Add(DataSourceSettings settings)
		{
			lock (_syncRoot)
			{
				var dataSource = new DataSource(settings);
				_dataSources.Add(dataSource);
				return dataSource;
			}
		}

		public DataSource Add(string fileName)
		{
			string fullFileName;
			var key = GetKey(fileName, out fullFileName);
			DataSource dataSource;

			lock (_syncRoot)
			{
				dataSource =
					_dataSources.FirstOrDefault(x => string.Equals(x.FullFileName, key, StringComparison.InvariantCultureIgnoreCase));
				if (dataSource == null)
				{
					var settings = new DataSourceSettings(fullFileName);
					_settings.Add(settings);
					dataSource = Add(settings);
				}
			}

			return dataSource;
		}

		private static string GetKey(string fileName, out string fullFileName)
		{
			fullFileName = Path.GetFullPath(fileName);
			var key = fullFileName.ToLower();
			return key;
		}

		public bool Remove(DataSource dataSource)
		{
			lock (_syncRoot)
			{
				_settings.Remove(dataSource.Settings);
				if (_dataSources.Remove(dataSource))
				{
					dataSource.Dispose();
					return true;
				}

				return false;
			}
		}

		public IEnumerator<DataSource> GetEnumerator()
		{
			lock (_syncRoot)
			{
				return _dataSources.ToList().GetEnumerator();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Dispose()
		{
			lock (_syncRoot)
			{
				foreach (var dataSource in _dataSources)
				{
					dataSource.Dispose();
				}
			}
		}
	}
}