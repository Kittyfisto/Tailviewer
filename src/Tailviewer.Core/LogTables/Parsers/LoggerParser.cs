using System.Text.RegularExpressions;

namespace Tailviewer.Core.LogTables.Parsers
{
	/// <summary>
	///     Not implemented yet.
	/// </summary>
	public sealed class LoggerParser
		: ColumnParser
	{
		private readonly Regex _expression;

		/// <summary>
		///     Initializes this parser.
		/// </summary>
		public LoggerParser()
		{
			_expression = new Regex(@"([.\w]+\.)?(\w+)", RegexOptions.Compiled);
		}

		/// <inheritdoc />
		public override object Parse(string line, int startIndex, out int numCharactersConsumed)
		{
			var match = _expression.Match(line, startIndex);
			if (match.Success)
			{
				numCharactersConsumed = match.Length;
				return match.Value;
			}

			numCharactersConsumed = 0;
			return null;
		}
	}
}