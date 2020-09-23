using System.Text;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Formats.Serilog
{
	/// <summary>
	///     Represents a particular serilog format.
	/// </summary>
	/// <example>
	///     "{Timestamp:dd/MM/yyyy HH:mm:ss K} [{Level}] {Message}"
	/// </example>
	public sealed class SerilogFileFormat
		: ILogFileFormat
	{
		private readonly Encoding _encoding;
		private readonly string _format;
		private readonly string _name;
		private readonly SerilogFileParser _parser;

		public SerilogFileFormat(string name, string format, Encoding encoding)
		{
			_name = name;
			_format = format;
			_encoding = encoding;
			_parser = new SerilogFileParser(format);
		}

		public SerilogFileParser Parser
		{
			get { return _parser; }
		}

		#region Implementation of ILogFileFormat

		public string Name
		{
			get { return _name; }
		}

		public string Description
		{
			get { return _format; }
		}

		public bool IsText
		{
			get { return true; }
		}

		public Encoding Encoding
		{
			get { return _encoding; }
		}

		public string Format
		{
			get { return _format; }
		}

		#endregion
	}
}