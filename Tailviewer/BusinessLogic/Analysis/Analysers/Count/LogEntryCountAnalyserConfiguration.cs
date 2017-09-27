using System.Linq;
using System.Xml;
using Tailviewer.Core.Settings;

namespace Tailviewer.BusinessLogic.Analysis.Analysers.Count
{
	public sealed class LogEntryCountAnalyserConfiguration
		: LogAnalyserConfiguration
	{
		private readonly QuickFilters _quickFilters;

		public LogEntryCountAnalyserConfiguration()
		{
			_quickFilters = new QuickFilters();
		}

		public QuickFilters QuickFilters => _quickFilters;

		protected override void RestoreInternal(XmlReader reader)
		{
			
		}

		protected override void SaveInternal(XmlWriter writer)
		{
			
		}
		
		public override int GetHashCode()
		{
			return 412231;
		}

		public override object Clone()
		{
			var clone = new LogEntryCountAnalyserConfiguration();
			clone.QuickFilters.AddRange(_quickFilters.Select(x => x.Clone()));
			return clone;
		}

		public override bool IsEquivalent(ILogAnalyserConfiguration obj)
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
	}
}