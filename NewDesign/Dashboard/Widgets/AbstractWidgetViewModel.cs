using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NewDesign.Dashboard.Widgets
{
	public abstract class AbstractWidgetViewModel
		: IWidgetViewModel
	{
		private bool _isEditing;

		public bool IsEditing
		{
			get { return _isEditing; }
			set
			{
				if (value == _isEditing)
					return;

				_isEditing = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}