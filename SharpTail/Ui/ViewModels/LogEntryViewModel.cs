namespace SharpTail.Ui.ViewModels
{
	public sealed class LogEntryViewModel
	{
		private readonly int _index;
		private readonly string _message;

		public LogEntryViewModel(string message)
		{
			_message = message;
		}

		public LogEntryViewModel(int index, string message)
		{
			_index = index;
			_message = message;
		}

		public int Index
		{
			get { return _index; }
		}

		public string Message
		{
			get { return _message; }
		}
	}
}