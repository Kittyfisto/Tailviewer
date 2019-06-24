using Tailviewer.Analysis.Count.BusinessLogic;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.Templates.Analysis;

namespace Tailviewer.Analysis.Count.Ui
{
	public sealed class LogEntryCountWidgetViewModel
		: AbstractWidgetViewModel
	{
		private readonly LogEntryCountAnalyserConfiguration _analyserConfiguration;
		private readonly LogEntryCountWidgetConfiguration _widgetConfiguration;
		private readonly FiltersViewModel _quickFilters;
		private long? _count;

		public LogEntryCountWidgetViewModel(IServiceContainer services, IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser)
			: base(services, template, dataSourceAnalyser)
		{
			_analyserConfiguration = AnalyserConfiguration as LogEntryCountAnalyserConfiguration ?? new LogEntryCountAnalyserConfiguration();
			_widgetConfiguration = ViewConfiguration as LogEntryCountWidgetConfiguration ?? new LogEntryCountWidgetConfiguration();

			Title = "Line Count";
			Caption = "Line(s)";
			_quickFilters = new FiltersViewModel(_analyserConfiguration?.QuickFilters);
		}

		public long? Count
		{
			get { return _count; }
			private set
			{
				if (value == _count)
					return;

				_count = value;
				EmitPropertyChanged();
			}
		}

		public string Caption
		{
			get { return _widgetConfiguration.Caption; }
			set
			{
				if (value == Caption)
					return;

				_widgetConfiguration.Caption = value;
				EmitPropertyChanged();
				EmitTemplateModified();
			}
		}

		public FiltersViewModel Filters => _quickFilters;

		public override void Update()
		{
			LogEntryCountResult result;
			if (TryGetResult(out result))
			{
				Count = result.Count;
			}
			else
			{
				Count = null;
			}

			base.Update();
		}
	}
}