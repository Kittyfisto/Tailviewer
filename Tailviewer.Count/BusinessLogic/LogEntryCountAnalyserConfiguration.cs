using System.Linq;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Settings;

namespace Tailviewer.Count.BusinessLogic
{
	public sealed class LogEntryCountAnalyserConfiguration
		: ILogAnalyserConfiguration
	{
		private QuickFilters _quickFilters;

		public LogEntryCountAnalyserConfiguration()
		{
			_quickFilters = new QuickFilters();
		}

		public QuickFilters QuickFilters => _quickFilters;

		public override int GetHashCode()
		{
			return 412231;
		}

		public object Clone()
		{
			var clone = new LogEntryCountAnalyserConfiguration();
			clone.QuickFilters.AddRange(_quickFilters.Select(x => x.Clone()));
			return clone;
		}

		public bool IsEquivalent(ILogAnalyserConfiguration obj)
		{
			if (ReferenceEquals(obj, null))
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			var other = obj as LogEntryCountAnalyserConfiguration;
			if (ReferenceEquals(other, null))
				return false;

			if (!_quickFilters.IsEquivalent(other.QuickFilters))
				return false;

			return true;
		}

		public void Serialize(IWriter writer)
		{
			writer.WriteAttribute("QuickFilters", (ISerializableType)_quickFilters);
		}

		public void Deserialize(IReader reader)
		{
			reader.TryReadAttribute("QuickFilters", out _quickFilters);
		}
	}
}