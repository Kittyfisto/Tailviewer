using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Tailviewer.Ui.SidePanel
{
	public abstract class AbstractSidePanelViewModel
		: ISidePanelViewModel
	{
		private bool _isSelected;
		private string _quickInfo;
		private string _tooltip;

		public event PropertyChangedEventHandler PropertyChanged;

		public abstract Geometry Icon { get; }

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value == _isSelected)
					return;

				_isSelected = value;
				EmitPropertyChanged();
			}
		}

		public abstract string Id { get; }

		public string QuickInfo
		{
			get { return _quickInfo; }
			protected set
			{
				if (value == _quickInfo)
					return;

				_quickInfo = value;
				EmitPropertyChanged();
			}
		}

		public string Tooltip
		{
			get { return _tooltip; }
			protected set
			{
				if (value == _tooltip)
					return;

				_tooltip = value;
				EmitPropertyChanged();
			}
		}

		public abstract void Update();

		protected virtual void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}