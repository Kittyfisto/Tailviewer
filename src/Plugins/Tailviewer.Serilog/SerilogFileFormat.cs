using System.Text;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Serilog
{
	public sealed class SerilogFileFormat
		: ILogFileFormat
	{
		private readonly string _name;
		private readonly string _format;

		#region Implementation of ILogFileFormat

		public string Name
		{
			get { return _name; }
		}

		public string Description
		{
			get { return Name; }
		}

		public bool IsText
		{
			get { return true; }
		}

		public Encoding Encoding
		{
			get { return Encoding.Default; }
		}

		#endregion
	}
}