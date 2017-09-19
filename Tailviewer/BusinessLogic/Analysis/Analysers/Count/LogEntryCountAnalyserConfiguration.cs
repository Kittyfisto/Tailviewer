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

		public override object Clone()
		{
			throw new System.NotImplementedException();
		}
	}
}