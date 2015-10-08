namespace SharpTail.Ui.ViewModels
{
	public sealed class LogEntryViewModel
	{
		private readonly string _filterString;
		private readonly int _index;
		private readonly string _message;

		public LogEntryViewModel(string message)
		{
			_message = message;
		}

		public LogEntryViewModel(int index, string message, string filterString)
		{
			_index = index;
			_message = message;
			_filterString = filterString;
		}

		public LogEntryViewModel(string message, string filterString)
		{
			_message = message;
			_filterString = filterString;
		}

		public int Index
		{
			get { return _index; }
		}

		public string FilterString
		{
			get { return _filterString; }
		}

		public string Message
		{
			get { return _message; }
		}
	}
}