using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.ViewModels
{
	public sealed class LogEntryViewModel
	{
		private readonly int _index;
		private readonly string _message;
		private readonly LevelFlags _level;

		public LogEntryViewModel(string message)
		{
			_message = message;
		}

		public LogEntryViewModel(int index, LogLine line)
		{
			_index = index;
			_message = line.Message;
			_level = line.Level;
		}

		public LevelFlags Level
		{
			get { return _level; }
		}

		public int Index
		{
			get { return _index; }
		}

		public string Message
		{
			get { return _message; }
		}

		public override string ToString()
		{
			return _message;
		}
	}
}