using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse
{
	/// <summary>
	///     Represents a running analysis of one or more data sources.
	/// </summary>
	public interface IAnalysisViewModel
		: INotifyPropertyChanged
	{
		AnalysisId Id { get; }
		IEnumerable<AnalysisPageViewModel> Pages { get; }
		string Name { get; set; }
		ICommand AddPageCommand { get; }
	}
}