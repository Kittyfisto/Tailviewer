using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui.ViewModels
{
	public sealed class LogEntryViewModel
	{
		private readonly int _index;
		private readonly string _message;

		public LogEntryViewModel(string message)
		{
			_message = message;
		}

		public LogEntryViewModel(int index, LogEntry entry)
		{
			_index = index;
			_message = entry.Message;
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