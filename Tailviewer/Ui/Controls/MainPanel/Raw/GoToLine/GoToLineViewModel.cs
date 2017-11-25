using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui.Controls.MainPanel.Raw.GoToLine
{
	public sealed class GoToLineViewModel
		: INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private int? _lineNumber;
		private bool _show;

		public bool Show
		{
			get { return _show; }
			set
			{
				if (value == _show)
					return;

				_show = value;
				EmitPropertyChanged();

				if (value)
				{
					LineNumber = null;
				}
			}
		}

		public int? LineNumber
		{
			get { return _lineNumber; }
			set
			{
				if (value == _lineNumber)
					return;

				_lineNumber = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event Action<LogLineIndex> LineNumberChosen;

		public void ChoseLineNumber()
		{
			var lineNumber = _lineNumber;
			if (lineNumber != null)
			{
				// Line numbers start at 1, but indices start at 0...
				LineNumberChosen?.Invoke(new LogLineIndex(lineNumber.Value - 1));
			}
			else
			{
				Log.InfoFormat("No line number entered yet, ignoring key press");
			}
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}