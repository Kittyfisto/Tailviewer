using Tailviewer.BusinessLogic;

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

		public LogEntryViewModel(int index, LogEntry entry)
		{
			_index = index;
			_message = entry.Message;
			_level = entry.Level;
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