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

		public bool PassesFilter(LogEntry logEntry)
		{
			if ((logEntry.Level & Level) != 0)
				return true;

			if (logEntry.Level != LevelFlags.None || ExcludeOther)
				return false;

			return true;
		}
	}
}