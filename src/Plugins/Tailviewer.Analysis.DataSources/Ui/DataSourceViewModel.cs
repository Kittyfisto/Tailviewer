using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Metrolib;
using Tailviewer.Analysis.DataSources.BusinessLogic;

namespace Tailviewer.Analysis.DataSources.Ui
{
	public sealed class DataSourceViewModel
		: INotifyPropertyChanged
	{
		private Size? _size;
		private DateTime? _created;
		private DateTime? _lastModified;

		public DataSourceViewModel(DataSourceResult dataSourceResult, DataSourcesWidgetConfiguration viewConfiguration)
		{
			Id = dataSourceResult.Id;
			ViewConfiguration = viewConfiguration;
			try
			{
				Name = Path.GetFileName(dataSourceResult.Name);
			}
			catch (Exception)
			{
				Name = dataSourceResult.Name;
			}
		}

		public DataSourceId Id { get; }

		public DataSourcesWidgetConfiguration ViewConfiguration { get; }

		public string Name { get; }

		public Size? Size
		{
			get => _size;
			set
			{
				if (value == _size)
					return;

				_size = value;
				EmitPropertyChanged();
			}
		}

		public DateTime? Created
		{
			get => _created;
			set
			{
				if (value == _created)
					return;

				_created = value;
				EmitPropertyChanged();
			}
		}

		public DateTime? LastModified
		{
			get => _lastModified;
			set
			{
				if (value == _lastModified)
					return;

				_lastModified = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}