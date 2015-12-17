namespace Tailviewer.BusinessLogic
{
	/// <summary>
	/// A filter that can be used to exclude entries of certain levels.
	/// </summary>
	internal sealed class LevelFilter
		: IFilter
	{
		public readonly LevelFlags Level;
		public readonly bool ExcludeOther;

		public LevelFilter(LevelFlags level, bool excludeOther)
		{
			Level = level;
			ExcludeOther = excludeOther;
		}

		public bool PassesFilter(LogLine logLine)
		{
			if ((logLine.Level & Level) != 0)
				return true;

			if (logLine.Level != LevelFlags.None || ExcludeOther)
				return false;

			return true;
		}
	}
}