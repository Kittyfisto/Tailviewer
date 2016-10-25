using System.Text.RegularExpressions;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class LoggerParser
		: ColumnParser
	{
		private readonly Regex _expression;

		public LoggerParser()
		{
			_expression = new Regex(@"([.\w]+\.)?(\w+)", RegexOptions.Compiled);
		}

		public override ColumnType Type
		{
			get { return ColumnType.Logger; }
		}

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