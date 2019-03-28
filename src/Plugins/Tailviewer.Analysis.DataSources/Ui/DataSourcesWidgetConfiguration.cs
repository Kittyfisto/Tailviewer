using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Analysis.DataSources.Ui
{
	[DataContract]
	public sealed class DataSourcesWidgetConfiguration
		: IWidgetConfiguration
		, INotifyPropertyChanged
	{
		private bool _showFileName;
		private bool _showFileSize;
		private bool _showCreated;
		private bool _showLastModified;

		public bool ShowFileName
		{
			get => _showFileName;
			set
			{
				if (value == _showFileName)
					return;

				_showFileName = value;
				EmitPropertyChanged();
			}
		}

		public bool ShowFileSize
		{
			get => _showFileSize;
			set
			{
				if (value == _showFileSize)
					return;

				_showFileSize = value;
				EmitPropertyChanged();
			}
		}

		public bool ShowCreated
		{
			get => _showCreated;
			set
			{
				if (value == _showCreated)
					return;

				_showCreated = value;
				EmitPropertyChanged();
			}
		}

		public bool ShowLastModified
		{
			get => _showLastModified;
			set
			{
				if (value == _showLastModified)
					return;

				_showLastModified = value;
				EmitPropertyChanged();
			}
		}

		public DataSourcesWidgetConfiguration()
		{
			ShowFileName = true;
		}

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("ShowFileName", ShowFileName);
			writer.WriteAttribute("ShowFileSize", ShowFileSize);
			writer.WriteAttribute("ShowCreated", ShowCreated);
			writer.WriteAttribute("ShowLastModified", ShowLastModified);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("ShowFileName", out _showFileName);
			reader.TryReadAttribute("ShowFileSize", out _showFileSize);
			reader.TryReadAttribute("ShowCreated", out _showCreated);
			reader.TryReadAttribute("ShowLastModified", out _showLastModified);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public DataSourcesWidgetConfiguration Clone()
		{
			return new DataSourcesWidgetConfiguration
			{
				ShowFileName = ShowFileName,
				ShowFileSize = ShowFileSize,
				ShowCreated = ShowCreated,
				ShowLastModified = ShowLastModified
			};
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}